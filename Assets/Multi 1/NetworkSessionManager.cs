using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multi_1
{
    public enum SessionMode { None, Solo, Host, Client }

    /// <summary>
    /// Single entry point for networking (GDD section 3):
    ///  - Solo  : internal host on localhost (UnityTransport), the AI plays the second galaxy. No Steam needed.
    ///  - Host  : Steam lobby + host over the Steam transport (FacepunchTransport).
    ///  - Client: joins a Steam lobby (invite / "Join game" / friends list) and connects to the lobby's host.
    /// Put it on the same ROOT object as the NetworkManager and both transports.
    /// </summary>
    public class NetworkSessionManager : MonoBehaviour
    {
        public static NetworkSessionManager Instance { get; private set; }
        public const int MaxPlayers = 2;

        [Header("Netcode")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport localTransport;
        [SerializeField] private FacepunchTransport steamTransport;
        [SerializeField] private ushort localPort = 7777;

        [Header("Game")]
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string menuSceneName = "MainMenu";
        [Tooltip("Prefab with NetworkObject + PlayerNetwork. Must be in the NetworkPrefabs list.")]
        [SerializeField] private NetworkObject playerPrefab;

        public SessionMode Mode { get; private set; } = SessionMode.None;
        public bool MatchStarted { get; private set; }
        public bool IsRunning => networkManager != null && networkManager.IsListening;
        public int ConnectedPlayerCount => networkManager.IsServer ? networkManager.ConnectedClientsIds.Count : 0;

        public event Action<SessionMode> SessionStarted;
        public event Action SessionEnded;
        public event Action<string> SessionFailed;
        public event Action PlayerCountChanged;
        public event Action OpponentLeft;

        private bool _shuttingDown;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            networkManager.OnClientConnectedCallback += _ => PlayerCountChanged?.Invoke();
            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private async void Start()
        {
            if (SteamLobbyService.Instance != null)
                SteamLobbyService.Instance.JoinRequested += lobby => { _ = JoinLobbyAsync(lobby.Id); };

            // Game launched by Steam through an invite ("+connect_lobby <id>").
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "+connect_lobby" && ulong.TryParse(args[i + 1], out var id) && SteamService.IsReady)
                {
                    await JoinLobbyAsync(id);
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------------ START

        /// <summary>Solo = internal server on localhost; the opponent is an AI player spawned by the server.</summary>
        public bool StartSolo()
        {
            if (IsRunning) return false;
            Configure(localTransport);
            localTransport.SetConnectionData("127.0.0.1", localPort);
            Mode = SessionMode.Solo;

            if (!StartHostInternal()) return Fail("Could not start the local server.");
            SessionStarted?.Invoke(Mode);
            return StartMatch();
        }

        /// <summary>Creates a Steam lobby then starts the host. Friends can now be invited.</summary>
        public async Task<bool> HostOnlineAsync(bool friendsOnly = true)
        {
            if (IsRunning) return false;
            if (!SteamService.IsReady) return Fail("Steam is not running.");

            var lobby = await SteamLobbyService.Instance.CreateLobbyAsync(MaxPlayers, friendsOnly);
            if (!lobby.HasValue) return Fail("Could not create the Steam lobby.");

            Configure(steamTransport);
            Mode = SessionMode.Host;
            if (!StartHostInternal())
            {
                SteamLobbyService.Instance.LeaveLobby();
                Mode = SessionMode.None;
                return Fail("Could not start the host.");
            }
            SessionStarted?.Invoke(Mode);
            return true;
        }

        /// <summary>Joins a Steam lobby (from an invite, "Join game" or the friends list) and connects to its host.</summary>
        public async Task<bool> JoinLobbyAsync(SteamId lobbyId)
        {
            if (IsRunning) return false;
            if (!SteamService.IsReady) return Fail("Steam is not running.");

            var joined = await SteamLobbyService.Instance.JoinLobbyAsync(lobbyId);
            if (!joined.HasValue) return Fail("Could not join the lobby (full or closed).");
            var lobby = joined.Value;

            if (lobby.GetData(SteamLobbyService.VersionKey) != Application.version)
            {
                SteamLobbyService.Instance.LeaveLobby();
                return Fail("The host runs a different game version.");
            }
            if (!SteamLobbyService.TryGetHost(lobby, out var hostId))
            {
                SteamLobbyService.Instance.LeaveLobby();
                return Fail("The lobby has no valid host.");
            }

            Configure(steamTransport);
            steamTransport.targetSteamId = hostId.Value;
            Mode = SessionMode.Client;

            if (!networkManager.StartClient())
            {
                SteamLobbyService.Instance.LeaveLobby();
                Mode = SessionMode.None;
                return Fail("Could not connect to the host.");
            }
            SessionStarted?.Invoke(Mode);
            return true;
        }

        /// <summary>Server only. Locks the lobby and loads the game scene on every client.</summary>
        public bool StartMatch()
        {
            if (!networkManager.IsServer || MatchStarted) return false;
            if (Mode == SessionMode.Host && ConnectedPlayerCount < MaxPlayers) return false;

            MatchStarted = true;
            if (Mode == SessionMode.Host) SteamLobbyService.Instance.SetJoinable(false);
            networkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            return true;
        }

        public void Disconnect()
        {
            if (_shuttingDown) return;
            _shuttingDown = true;

            if (networkManager.IsListening) networkManager.Shutdown();
            if (SteamLobbyService.Instance != null) SteamLobbyService.Instance.LeaveLobby();

            Mode = SessionMode.None;
            MatchStarted = false;
            _shuttingDown = false;
            SessionEnded?.Invoke();
        }

        // ------------------------------------------------------------------ INTERNALS

        private void Configure(NetworkTransport transport)
        {
            var cfg = networkManager.NetworkConfig;
            cfg.NetworkTransport = transport;
            cfg.ConnectionApproval = true;
            cfg.PlayerPrefab = null; // players are spawned manually (see SpawnPlayer)
            networkManager.ConnectionApprovalCallback = ApproveConnection;
        }

        private bool StartHostInternal()
        {
            MatchStarted = false;
            if (!networkManager.StartHost()) return false;
            networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            return true;
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request,
                                       NetworkManager.ConnectionApprovalResponse response)
        {
            bool full = networkManager.ConnectedClientsIds.Count >= MaxPlayers;
            response.Approved = !full && !MatchStarted;
            response.CreatePlayerObject = false;
            response.Pending = false;
            if (!response.Approved) response.Reason = full ? "Lobby full" : "Match already started";
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_shuttingDown) return;

            if (networkManager.IsServer)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    PlayerCountChanged?.Invoke();
                    OpponentLeft?.Invoke(); // the host decides what to do (win by forfeit, back to lobby...)
                }
            }
            else
            {
                // We lost the host.
                Disconnect();
                if (SceneManager.GetActiveScene().name != menuSceneName)
                    SceneManager.LoadScene(menuSceneName);
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode mode,
                                          List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!networkManager.IsServer || sceneName != gameSceneName) return;

            var ids = new List<ulong>(networkManager.ConnectedClientsIds);
            ids.Sort(); // host (id 0) always gets slot 0
            int slot = 0;
            foreach (var id in ids) SpawnPlayer(id, slot++, false);

            if (Mode == SessionMode.Solo) SpawnPlayer(NetworkManager.ServerClientId, slot, true); // AI, owned by the server
        }

        private void SpawnPlayer(ulong ownerClientId, int slot, bool isAI)
        {
            var obj = Instantiate(playerPrefab);
            obj.GetComponent<BlackHole2>().ServerInit(slot, isAI);
            obj.SpawnWithOwnership(ownerClientId, true);
        }

        private bool Fail(string message)
        {
            Debug.LogWarning($"[Net] {message}");
            SessionFailed?.Invoke(message);
            return false;
        }
    }
}
