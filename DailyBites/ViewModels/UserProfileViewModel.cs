using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using DailyBites.Services;

namespace DailyBites.ViewModels;

public partial class UserProfileViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();
    private readonly IFriendService _friendService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty] 
    private string _uid = string.Empty;
    [ObservableProperty] 
    private string _username = string.Empty;
    [ObservableProperty] 
    private string _name = string.Empty;
    [ObservableProperty] 
    private string _email = string.Empty;
    [ObservableProperty] 
    private string _profilePicUrl = string.Empty;
    [ObservableProperty] 
    private int _friendCount;

    [ObservableProperty] 
    private string _friendButtonText = "Add Friend";
    [ObservableProperty] 
    private bool _showFriendButton = true;
    [ObservableProperty] 
    private bool _showAcceptReject = false;

    public IAsyncRelayCommand FriendButtonCommand { get; }
    public IAsyncRelayCommand AcceptCommand { get; }
    public IAsyncRelayCommand RejectCommand { get; }

    public UserProfileViewModel(IConfiguration config, IFriendService friendService, ISettingsService settingsService)
    {
        _config = config;
        _friendService = friendService;
        _settingsService = settingsService;

        FriendButtonCommand = new AsyncRelayCommand(OnFriendButtonClicked);
        AcceptCommand = new AsyncRelayCommand(OnAcceptClicked);
        RejectCommand = new AsyncRelayCommand(OnRejectClicked);
    }

    public async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(Uid)) return;

        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{Uid}";

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode) return;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        if (!json.RootElement.TryGetProperty("fields", out var fields)) return;

        string GetString(string key)
        {
            return fields.TryGetProperty(key, out var f) && f.TryGetProperty("stringValue", out var sv)
                ? sv.GetString() ?? string.Empty
                : string.Empty;
        }

        Username = GetString("username");
        Name = GetString("name");
        Email = GetString("email");
        ProfilePicUrl = GetString("profilePicUrl");

        // ----- Check friends -----
        if (fields.TryGetProperty("friends", out var friendsField) &&
            friendsField.TryGetProperty("arrayValue", out var arr) &&
            arr.TryGetProperty("values", out var vals))
        {
            FriendCount = vals.GetArrayLength();

            if (vals.EnumerateArray().Any(v => v.GetProperty("stringValue").GetString() == _settingsService.Uid))
            {
                FriendButtonText = "Remove Friend";
                ShowFriendButton = true;
                ShowAcceptReject = false;
                return;
            }
        }
        else
        {
            FriendCount = 0;
        }

        // ----- Did YOU send them a request? -----
        if (fields.TryGetProperty("friendRequests", out var requestsField) &&
            requestsField.TryGetProperty("arrayValue", out var reqArr) &&
            reqArr.TryGetProperty("values", out var reqVals))
        {
            foreach (var v in reqVals.EnumerateArray())
            {
                var requesterUid = v.GetProperty("stringValue").GetString();

                if (requesterUid == _settingsService.Uid)
                {
                    FriendButtonText = "Request Sent";
                    ShowFriendButton = true;
                    ShowAcceptReject = false;
                    return;
                }

                if (requesterUid == Uid && Uid == _settingsService.Uid)
                {
                    // edge case, ignore
                }

                // 🔹 If THEY sent YOU a request
                if (requesterUid == _settingsService.Uid) continue;
                if (Uid == _settingsService.Uid) continue;
            }
        }

        // ----- Did THEY send YOU a request? -----
        var myUrl = $"{projectId}/databases/(default)/documents/users/{_settingsService.Uid}";
        var myRes = await _http.GetAsync($"https://firestore.googleapis.com/v1/projects/{myUrl}");
        if (myRes.IsSuccessStatusCode)
        {
            using var myJson = await JsonDocument.ParseAsync(await myRes.Content.ReadAsStreamAsync());
            if (myJson.RootElement.TryGetProperty("fields", out var myFields))
            {
                if (myFields.TryGetProperty("friendRequests", out var myReqField) &&
                    myReqField.TryGetProperty("arrayValue", out var myReqArr) &&
                    myReqArr.TryGetProperty("values", out var myReqVals))
                {
                    foreach (var v in myReqVals.EnumerateArray())
                    {
                        if (v.TryGetProperty("stringValue", out var sv) &&
                            sv.GetString() == Uid)
                        {
                            ShowFriendButton = false;
                            ShowAcceptReject = true;
                            return;
                        }
                    }
                }
            }
        }

        // Default
        FriendButtonText = "Add Friend";
        ShowFriendButton = true;
        ShowAcceptReject = false;
    }

    private async Task OnFriendButtonClicked()
    {
        if (FriendButtonText == "Add Friend")
        {
            var ok = await _friendService.SendFriendRequestAsync(_settingsService.Uid, Uid);
            if (ok) FriendButtonText = "Request Sent";
        }
        else if (FriendButtonText == "Remove Friend")
        {
            var ok = await _friendService.RemoveFriendAsync(_settingsService.Uid, Uid);
            if (ok) FriendButtonText = "Add Friend";
        }
        else if (FriendButtonText == "Request Sent")
        {
            var ok = await _friendService.RejectFriendRequestAsync(Uid, _settingsService.Uid);
            if (ok) FriendButtonText = "Add Friend";
        }
    }

    private async Task OnAcceptClicked()
    {
        var ok = await _friendService.AcceptFriendRequestAsync(_settingsService.Uid, Uid);
        if (ok)
        {
            FriendButtonText = "Remove Friend";
            ShowFriendButton = true;
            ShowAcceptReject = false;
        }
    }

    private async Task OnRejectClicked()
    {
        var ok = await _friendService.RejectFriendRequestAsync(_settingsService.Uid, Uid);
        if (ok)
        {
            FriendButtonText = "Add Friend";
            ShowFriendButton = true;
            ShowAcceptReject = false;
        }
    }
}
