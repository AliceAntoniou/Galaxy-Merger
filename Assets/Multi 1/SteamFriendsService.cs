using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Multi_1
{
    public struct FriendEntry
    {
        public SteamId Id;
        public string Name;
        public bool IsOnline;
        public bool IsPlayingThisGame;
        public bool IsInOtherGame;
    }

    /// <summary>Read the Steam friends list and invite friends into the current lobby.</summary>
    public static class SteamFriendsService
    {
        /// <summary>Friends sorted: playing this game, then online, then offline (A-Z inside each group).</summary>
        public static List<FriendEntry> GetFriends()
        {
            var result = new List<FriendEntry>();
            if (!SteamService.IsReady) return result;

            foreach (var f in SteamFriends.GetFriends())
            {
                result.Add(new FriendEntry
                {
                    Id = f.Id,
                    Name = f.Name,
                    IsOnline = f.IsOnline,
                    IsPlayingThisGame = f.IsPlayingThisGame,
                    IsInOtherGame = f.GameInfo.HasValue && !f.IsPlayingThisGame
                });
            }

            result.Sort((a, b) =>
            {
                int c = Rank(a).CompareTo(Rank(b));
                return c != 0 ? c : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            return result;
        }

        private static int Rank(FriendEntry e) => e.IsPlayingThisGame ? 0 : e.IsOnline ? 1 : 2;

        /// <summary>Sends a Steam invite for the current lobby to one friend.</summary>
        public static bool Invite(SteamId friend)
        {
            var lobby = SteamLobbyService.Instance != null ? SteamLobbyService.Instance.CurrentLobby : null;
            return lobby.HasValue && lobby.Value.InviteFriend(friend);
        }

        /// <summary>Opens Steam's own invite overlay for the current lobby (alternative to a custom list).</summary>
        public static void OpenInviteOverlay()
        {
            var lobby = SteamLobbyService.Instance != null ? SteamLobbyService.Instance.CurrentLobby : null;
            if (lobby.HasValue) SteamFriends.OpenGameInviteOverlay(lobby.Value.Id);
        }

        /// <summary>Downloads a friend's avatar as a Texture2D (flipped for Unity). Returns null if unavailable.</summary>
        public static async Task<Texture2D> GetAvatarAsync(SteamId id)
        {
            var img = await SteamFriends.GetMediumAvatarAsync(id);
            if (!img.HasValue) return null;

            var i = img.Value;
            int w = (int)i.Width, h = (int)i.Height, stride = w * 4;
            var flipped = new byte[i.Data.Length];
            for (int y = 0; y < h; y++)
                Buffer.BlockCopy(i.Data, y * stride, flipped, (h - 1 - y) * stride, stride);

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(flipped);
            tex.Apply();
            return tex;
        }
    }
}
