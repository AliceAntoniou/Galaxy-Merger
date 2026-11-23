using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Multi_1
{
    /// <summary>Scroll list of Steam friends with an "Invite" button. Auto-refreshes when a friend's status changes.</summary>
    public class FriendsListUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private FriendEntryUI entryPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private bool onlyOnline = false;

        private bool _dirty;
        private float _dirtyTime;
        private bool _subscribed;

        private void OnEnable()
        {
            if (refreshButton != null) refreshButton.onClick.AddListener(Refresh);
            if (SteamService.IsReady && !_subscribed)
            {
                SteamFriends.OnPersonaStateChange += OnPersonaChanged;
                _subscribed = true;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (refreshButton != null) refreshButton.onClick.RemoveListener(Refresh);
            if (_subscribed)
            {
                SteamFriends.OnPersonaStateChange -= OnPersonaChanged;
                _subscribed = false;
            }
        }

        private void OnPersonaChanged(Friend friend)
        {
            _dirty = true;
            _dirtyTime = Time.unscaledTime + 1f; // debounce: Steam fires this a lot
        }

        private void Update()
        {
            if (_dirty && Time.unscaledTime >= _dirtyTime) Refresh();
        }

        public void Refresh()
        {
            _dirty = false;
            foreach (Transform child in content) Destroy(child.gameObject);

            foreach (var entry in SteamFriendsService.GetFriends())
            {
                if (onlyOnline && !entry.IsOnline) continue;
                Instantiate(entryPrefab, content).Bind(entry);
            }
        }
    }
}
