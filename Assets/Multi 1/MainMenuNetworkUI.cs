using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multi_1
{
    /// <summary>
    /// Example glue between the main menu and NetworkSessionManager.
    /// Solo -> internal server + AI. Host -> Steam lobby + friends list. Start -> loads the game for both players.
    /// A guest who accepts an invite ("Join game" in Steam) ends up in the lobby panel automatically.
    /// </summary>
    public class MainMenuNetworkUI : MonoBehaviour
    {
        [SerializeField] private Button soloButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button startMatchButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text membersText;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject lobbyPanel; // contains the FriendsListUI

        private NetworkSessionManager Session => NetworkSessionManager.Instance;

        private void Start()
        {
            soloButton.onClick.AddListener(() => Session.StartSolo());
            hostButton.onClick.AddListener(async () => await Session.HostOnlineAsync());
            startMatchButton.onClick.AddListener(() => Session.StartMatch());
            leaveButton.onClick.AddListener(() => Session.Disconnect());

            Session.SessionStarted += OnSessionStarted;
            Session.SessionEnded += ShowMain;
            Session.SessionFailed += ShowStatus;
            Session.PlayerCountChanged += RefreshLobby;

            hostButton.interactable = SteamService.IsReady; // solo works without Steam
            ShowMain();
        }

        private void OnDestroy()
        {
            if (Session == null) return;
            Session.SessionStarted -= OnSessionStarted;
            Session.SessionEnded -= ShowMain;
            Session.SessionFailed -= ShowStatus;
            Session.PlayerCountChanged -= RefreshLobby;
        }

        private void OnSessionStarted(SessionMode mode)
        {
            if (mode == SessionMode.Solo) return; // the scene loads right away
            mainPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            RefreshLobby();
        }

        private void ShowMain()
        {
            mainPanel.SetActive(true);
            lobbyPanel.SetActive(false);
        }

        private void RefreshLobby()
        {
            var sb = new StringBuilder();
            var lobby = SteamLobbyService.Instance != null ? SteamLobbyService.Instance.CurrentLobby : null;
            if (lobby.HasValue)
                foreach (var m in lobby.Value.Members) sb.AppendLine(m.Name);
            membersText.text = sb.ToString();

            bool isHost = Session.Mode == SessionMode.Host;
            startMatchButton.gameObject.SetActive(isHost);
            startMatchButton.interactable = isHost && Session.ConnectedPlayerCount >= NetworkSessionManager.MaxPlayers;
            statusText.text = isHost ? "Invite a friend, then press Start." : "Waiting for the host to start...";
        }

        private void ShowStatus(string message) => statusText.text = message;
    }
}
