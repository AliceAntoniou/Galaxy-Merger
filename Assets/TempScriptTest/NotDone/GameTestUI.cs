using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Multi_1
{
    /// <summary>
    /// Minimal in-game panel to test the netcode: shows every black hole (slot, AI/human, size, stardust),
    /// lets the local player upgrade, add a test tile, or leave the match.
    /// </summary>
    public class GameTestUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button addTileButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private string menuSceneName = "MainMenu";

        private readonly StringBuilder _sb = new();

        private void Start()
        {
            upgradeButton.onClick.AddListener(() =>
            {
                var mine = BlackHole2.GetLocalPlayer();
                if (mine != null) mine.RequestUpgradeRpc();
            });

            addTileButton.onClick.AddListener(() =>
            {
                if (GameManager.instance != null) GameManager.instance.TestAddTileToBlackHole();
            });

            leaveButton.onClick.AddListener(() =>
            {
                if (NetworkSessionManager.Instance != null) NetworkSessionManager.Instance.Disconnect();
                SceneManager.LoadScene(menuSceneName);
            });
        }

        private void Update()
        {
            _sb.Clear();
            _sb.AppendLine(GameManager.instance != null && GameManager.instance.GameStarted
                ? "Game started" : "Waiting for all black holes...");

            foreach (var b in BlackHole2.All)
            {
                _sb.Append("Slot ").Append(b.Slot.Value)
                   .Append(b.IsAI.Value ? " [AI]" : " [Human]")
                   .Append(b.IsLocalHuman ? " (you)" : "")
                   .Append("  size ").Append(b.GetSize())
                   .Append("  stardust ").Append(b.stardust)
                   .Append("  next upgrade ").Append(b.GetUpgradePrice())
                   .AppendLine();
            }
            infoText.text = _sb.ToString();
        }
    }
}
