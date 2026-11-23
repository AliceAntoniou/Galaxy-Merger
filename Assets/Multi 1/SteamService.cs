using System;
using Steamworks;
using UnityEngine;

namespace Multi_1
{
    /// <summary>
    /// Initialises Steamworks (Facepunch.Steamworks) and pumps its callbacks on the main thread.
    /// Put it on a ROOT object of your bootstrap scene.
    /// The game still works offline (solo) when Steam is not running: IsReady will simply be false.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SteamService : MonoBehaviour
    {
        public static SteamService Instance { get; private set; }
        public static bool IsReady { get; private set; }

        [Tooltip("480 = Spacewar (Valve test app). Replace with the real AppId once you have one.")]
        [SerializeField] private uint appId = 480;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                // false = we call RunCallbacks() ourselves from Update, so every Steam event
                // (lobby, friends...) is raised on the Unity main thread.
                SteamClient.Init(appId, false);
                IsReady = SteamClient.IsValid;
                if (IsReady) Debug.Log($"[Steam] Logged in as {SteamClient.Name} ({SteamClient.SteamId})");
            }
            catch (Exception e)
            {
                IsReady = false;
                Debug.LogWarning($"[Steam] Init failed (is the Steam client running?): {e.Message}");
            }
        }

        private void Update()
        {
            if (IsReady) SteamClient.RunCallbacks();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            if (IsReady) SteamClient.Shutdown();
            IsReady = false;
            Instance = null;
        }
    }
}
