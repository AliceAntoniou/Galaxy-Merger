using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Multi_1
{
    /// <summary>
    /// Wraps the Steam lobby used as "waiting room": friends are invited into the lobby,
    /// the lobby stores the host's SteamId, then Netcode connects through the Steam transport.
    /// </summary>
    public class SteamLobbyService : MonoBehaviour
    {
        public static SteamLobbyService Instance { get; private set; }

        public const string HostKey = "host";
        public const string VersionKey = "ver";

        public Lobby? CurrentLobby { get; private set; }
        public bool InLobby => CurrentLobby.HasValue;

        public event Action<Lobby> LobbyEntered;
        public event Action LobbyLeft;
        public event Action<Lobby, Friend> MemberJoined;
        public event Action<Lobby, Friend> MemberLeft;
        /// <summary>Raised when the player clicks "Join game" from Steam (overlay, friends list, invite toast).</summary>
        public event Action<Lobby> JoinRequested;

        private bool _subscribed;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!SteamService.IsReady) return;
            SteamMatchmaking.OnLobbyEntered += HandleEntered;
            SteamMatchmaking.OnLobbyMemberJoined += HandleMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += HandleMemberLeft;
            SteamFriends.OnGameLobbyJoinRequested += HandleJoinRequested;
            _subscribed = true;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            if (_subscribed)
            {
                SteamMatchmaking.OnLobbyEntered -= HandleEntered;
                SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
                SteamMatchmaking.OnLobbyMemberLeave -= HandleMemberLeft;
                SteamFriends.OnGameLobbyJoinRequested -= HandleJoinRequested;
            }
            LeaveLobby();
            Instance = null;
        }

        // ---------- API ----------

        public async Task<Lobby?> CreateLobbyAsync(int maxMembers = 2, bool friendsOnly = true)
        {
            if (!SteamService.IsReady) return null;
            LeaveLobby();

            var created = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
            if (!created.HasValue) return null;

            var lobby = created.Value;
            if (friendsOnly) lobby.SetFriendsOnly(); else lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetData(HostKey, SteamClient.SteamId.Value.ToString());
            lobby.SetData(VersionKey, Application.version);

            CurrentLobby = lobby;
            return lobby;
        }

        public async Task<Lobby?> JoinLobbyAsync(SteamId lobbyId)
        {
            if (!SteamService.IsReady) return null;
            LeaveLobby();

            var joined = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
            if (joined.HasValue) CurrentLobby = joined.Value;
            return joined;
        }

        public void LeaveLobby()
        {
            if (!CurrentLobby.HasValue) return;
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
            LobbyLeft?.Invoke();
        }

        public void SetJoinable(bool joinable)
        {
            if (CurrentLobby.HasValue) CurrentLobby.Value.SetJoinable(joinable);
        }

        public static bool TryGetHost(Lobby lobby, out SteamId hostId)
        {
            hostId = default;
            if (!ulong.TryParse(lobby.GetData(HostKey), out var raw) || raw == 0) return false;
            hostId = raw;
            return true;
        }

        // ---------- Steam callbacks ----------

        private void HandleEntered(Lobby lobby)
        {
            CurrentLobby = lobby;
            LobbyEntered?.Invoke(lobby);
        }

        private void HandleMemberJoined(Lobby lobby, Friend friend) => MemberJoined?.Invoke(lobby, friend);
        private void HandleMemberLeft(Lobby lobby, Friend friend) => MemberLeft?.Invoke(lobby, friend);
        private void HandleJoinRequested(Lobby lobby, SteamId friendId) => JoinRequested?.Invoke(lobby);
    }
}
