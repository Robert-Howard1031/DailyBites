using System.Threading.Tasks;
using System.Collections.Generic;

namespace DailyBites.Services
{
    public interface IFriendService
    {
        Task<bool> SendFriendRequestAsync(string fromUid, string toUid);
        Task<bool> AcceptFriendRequestAsync(string currentUid, string requesterUid);
        Task<bool> RejectFriendRequestAsync(string currentUid, string requesterUid);
        Task<bool> RemoveFriendAsync(string currentUid, string friendUid);
        Task<List<string>> GetFriendsAsync(string uid);
        Task<List<string>> GetFriendRequestsAsync(string uid);
        Task<bool> AreFriendsAsync(string currentUid, string otherUid);
    }
}
