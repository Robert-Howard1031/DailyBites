using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyBites.Services
{
    public class FriendService : IFriendService
    {
        private readonly HttpClient _http = new();
        private readonly IConfiguration _config;

        public FriendService(IConfiguration config)
        {
            _config = config;
        }

        private string ProjectId => _config["Firebase:ProjectId"];
        private string BaseUrl => $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/users";

        // ✅ Send Friend Request
        public async Task<bool> SendFriendRequestAsync(string fromUid, string toUid)
        {
            var url = $"{BaseUrl}/{toUid}?updateMask.fieldPaths=friendRequests";
            var body = new
            {
                fields = new
                {
                    friendRequests = new
                    {
                        arrayValue = new
                        {
                            values = new[]
                            {
                                new { stringValue = fromUid }
                            }
                        }
                    }
                }
            };

            var res = await _http.PatchAsJsonAsync(url, body);
            return res.IsSuccessStatusCode;
        }

        // ✅ Accept Friend Request
        public async Task<bool> AcceptFriendRequestAsync(string currentUid, string requesterUid)
        {
            // Add requesterUid to current user’s friends
            var addToCurrent = await AddFriendAsync(currentUid, requesterUid);
            // Add currentUid to requester’s friends
            var addToRequester = await AddFriendAsync(requesterUid, currentUid);
            // Remove from pending requests
            var removeRequest = await RemoveRequestAsync(currentUid, requesterUid);

            return addToCurrent && addToRequester && removeRequest;
        }

        // ✅ Reject Friend Request
        public async Task<bool> RejectFriendRequestAsync(string currentUid, string requesterUid)
        {
            return await RemoveRequestAsync(currentUid, requesterUid);
        }

        // ✅ Remove Friend
        public async Task<bool> RemoveFriendAsync(string currentUid, string friendUid)
        {
            var removeFromCurrent = await RemoveFriendFromUser(currentUid, friendUid);
            var removeFromFriend = await RemoveFriendFromUser(friendUid, currentUid);
            return removeFromCurrent && removeFromFriend;
        }

        // ✅ Get Friends List
        public async Task<List<string>> GetFriendsAsync(string uid)
        {
            var url = $"{BaseUrl}/{uid}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new List<string>();

            using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            if (!json.RootElement.TryGetProperty("fields", out var fields)) return new List<string>();

            return ExtractArray(fields, "friends");
        }

        // ✅ Get Friend Requests
        public async Task<List<string>> GetFriendRequestsAsync(string uid)
        {
            var url = $"{BaseUrl}/{uid}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new List<string>();

            using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            if (!json.RootElement.TryGetProperty("fields", out var fields)) return new List<string>();

            return ExtractArray(fields, "friendRequests");
        }

        // ========== Helpers ==========
        private async Task<bool> AddFriendAsync(string uid, string friendUid)
        {
            var url = $"{BaseUrl}/{uid}?updateMask.fieldPaths=friends";
            var body = new
            {
                fields = new
                {
                    friends = new
                    {
                        arrayValue = new
                        {
                            values = new[]
                            {
                                new { stringValue = friendUid }
                            }
                        }
                    }
                }
            };
            var res = await _http.PatchAsJsonAsync(url, body);
            return res.IsSuccessStatusCode;
        }

        private async Task<bool> RemoveRequestAsync(string uid, string requesterUid)
        {
            // For now: overwrite with empty array (simplest).
            // Later: we can use Firestore transforms for arrayRemove.
            var url = $"{BaseUrl}/{uid}?updateMask.fieldPaths=friendRequests";
            var body = new
            {
                fields = new
                {
                    friendRequests = new
                    {
                        arrayValue = new { values = Array.Empty<object>() }
                    }
                }
            };
            var res = await _http.PatchAsJsonAsync(url, body);
            return res.IsSuccessStatusCode;
        }

        private async Task<bool> RemoveFriendFromUser(string uid, string friendUid)
        {
            var url = $"{BaseUrl}/{uid}?updateMask.fieldPaths=friends";
            var body = new
            {
                fields = new
                {
                    friends = new
                    {
                        arrayValue = new { values = Array.Empty<object>() }
                    }
                }
            };
            var res = await _http.PatchAsJsonAsync(url, body);
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> AreFriendsAsync(string currentUid, string otherUid)
        {
            var url = $"{BaseUrl}/{currentUid}";
            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return false;

            using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            if (!json.RootElement.TryGetProperty("fields", out var fields)) return false;

            var friends = ExtractArray(fields, "friends");
            return friends.Contains(otherUid);
        }


        private List<string> ExtractArray(JsonElement fields, string key)
        {
            var list = new List<string>();
            if (fields.TryGetProperty(key, out var arr) &&
                arr.TryGetProperty("arrayValue", out var av) &&
                av.TryGetProperty("values", out var values))
            {
                foreach (var v in values.EnumerateArray())
                {
                    if (v.TryGetProperty("stringValue", out var sv))
                        list.Add(sv.GetString() ?? "");
                }
            }
            return list;
        }
    }
}
