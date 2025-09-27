using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Models;
using DailyBites.Services;
using DailyBites.Views;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class FriendsListViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _http = new();

    [ObservableProperty] 
    private string _uid = string.Empty;
    [ObservableProperty]
    private string _stack = string.Empty;
    [ObservableProperty] private ObservableCollection<UserResult> _friends = new();
    [ObservableProperty] private UserResult? _selectedFriend;

    public FriendsListViewModel(IConfiguration config, ISettingsService settingsService)
    {
        _config = config;
        _settingsService = settingsService;
    }

    public async Task LoadAsync()
    {
        Friends.Clear();
        if (string.IsNullOrEmpty(Uid)) return;

        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{Uid}";

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode) return;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        if (!json.RootElement.TryGetProperty("fields", out var fields)) return;

        if (fields.TryGetProperty("friends", out var friendsField) &&
            friendsField.TryGetProperty("arrayValue", out var arr) &&
            arr.TryGetProperty("values", out var vals))
        {
            foreach (var v in vals.EnumerateArray())
            {
                var friendUid = v.GetProperty("stringValue").GetString();
                if (string.IsNullOrEmpty(friendUid)) continue;

                var fUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{friendUid}";
                var fRes = await _http.GetAsync(fUrl);
                if (!fRes.IsSuccessStatusCode) continue;

                using var fJson = await JsonDocument.ParseAsync(await fRes.Content.ReadAsStreamAsync());
                if (!fJson.RootElement.TryGetProperty("fields", out var fFields)) continue;

                string GetString(string key) =>
                    fFields.TryGetProperty(key, out var f) &&
                    f.TryGetProperty("stringValue", out var sv)
                        ? sv.GetString() ?? string.Empty
                        : string.Empty;

                Friends.Add(new UserResult
                {
                    Uid = GetString("uid"),
                    Username = GetString("username"),
                    Name = GetString("name"),
                    Email = GetString("email"),
                    ProfilePicUrl = GetString("profilePicUrl"),
                    FriendButtonText = "Friend"
                });
            }
        }
    }

    partial void OnSelectedFriendChanged(UserResult? value)
    {
        if (value is null) return;
        if(Stack == "search")
        {
            _ = Shell.Current.GoToAsync(
            $"//SearchPage/{nameof(UserProfilePage)}",
            true,
            new Dictionary<string, object>
            {
                ["uid"] = value.Uid,
                ["stack"] = "search"
            });
        }
        if (Stack == "personal")
        {
            _ = Shell.Current.GoToAsync(
            $"//PersonalProfilePage/{nameof(UserProfilePage)}",
            true,
            new Dictionary<string, object>
            {
                ["uid"] = value.Uid,
                ["stack"] = "personal"
            });
        }

        SelectedFriend = null;
    }
}
