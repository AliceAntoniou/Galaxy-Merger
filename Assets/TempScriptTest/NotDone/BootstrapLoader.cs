using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multi_1
{
    /// <summary>
    /// Bootstrap scene only. Put it on its OWN object (not on the "Network" object, which survives scene loads),
    /// otherwise it would reload the menu every time.
    /// Waits one frame so SteamService / SteamLobbyService / NetworkSessionManager finished their Awake and Start.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string firstScene = "MainMenu";

        private IEnumerator Start()
        {
            yield return null;
            SceneManager.LoadScene(firstScene);
        }
    }
}
