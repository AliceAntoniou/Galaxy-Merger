using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multi_1
{
    /// <summary>One row: avatar, name, status, Invite button.</summary>
    public class FriendEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private RawImage avatar;
        [SerializeField] private Button inviteButton;

        public async void Bind(FriendEntry entry)
        {
            nameText.text = entry.Name;
            statusText.text = entry.IsPlayingThisGame ? "Playing Galaxy Merger"
                            : entry.IsInOtherGame ? "In another game"
                            : entry.IsOnline ? "Online" : "Offline";

            bool canInvite = entry.IsOnline && SteamLobbyService.Instance && SteamLobbyService.Instance.InLobby;
            inviteButton.interactable = canInvite;
            inviteButton.onClick.AddListener(() =>
            {
                if (SteamFriendsService.Invite(entry.Id)) statusText.text = "Invitation sent";
            });

            var tex = await SteamFriendsService.GetAvatarAsync(entry.Id);
            if (!this) return; // row destroyed while the avatar was loading
            if (tex) avatar.texture = tex;
        }
    }
}
