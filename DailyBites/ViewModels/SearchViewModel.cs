using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Models;
using DailyBites.Services;
using DailyBites.Views;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly IFriendService _friendService;
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _http = new();

    [ObservableProperty] private string _query = string.Empty;
    [ObservableProperty] private ObservableCollection<UserResult> _results = new();
    [ObservableProperty] private UserResult? _selectedUser;

    public IAsyncRelayCommand<UserResult> ToggleFriendCommand { get; }

    public SearchViewModel(IConfiguration config, IFriendService friendService, ISettingsService settingsService)
    {
        _config = config;
        _friendService = friendService;
        _settingsService = settingsService;

        ToggleFriendCommand = new AsyncRelayCommand<UserResult>(ToggleFriend);
    }

    [RelayCommand]
    private async Task Search()
    {
        var q = (Query ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(q))
        {
            Results.Clear();
            return;
        }

        Results.Clear();

        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";
        var end = q + "\uf8ff";

        var body = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = "users" } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new object[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "username" },
                                    op = "GREATER_THAN_OR_EQUAL",
                                    value = new { stringValue = q }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "username" },
                                    op = "LESS_THAN_OR_EQUAL",
                                    value = new { stringValue = end }
                                }
                            }
                        }
                    }
                },
                limit = 25
            }
        };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());

        var friends = await _friendService.GetFriendsAsync(_settingsService.Uid);

        foreach (var element in json.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("document", out var doc)) continue;
            if (!doc.TryGetProperty("fields", out var fields)) continue;

            string GetString(string key)
            {
                return fields.TryGetProperty(key, out var f) && f.TryGetProperty("stringValue", out var sv)
                    ? sv.GetString() ?? string.Empty
                    : string.Empty;
            }

            var result = new UserResult
            {
                Uid = GetString("uid"),
                Username = GetString("username"),
                Name = GetString("name"),
                Email = GetString("email"),
                ProfilePicUrl = GetString("profilePicUrl"),
                FriendButtonText = "Add Friend"
            };

            if (result.Uid == _settingsService.Uid)
                continue;

            if (!string.IsNullOrWhiteSpace(result.Username))
            {
                if (friends.Contains(result.Uid))
                {
                    result.FriendButtonText = "Remove Friend";
                }
                else
                {
                    var userUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{result.Uid}";
                    var userRes = await _http.GetAsync(userUrl);
                    if (userRes.IsSuccessStatusCode)
                    {
                        using var userJson = await JsonDocument.ParseAsync(await userRes.Content.ReadAsStreamAsync());
                        if (userJson.RootElement.TryGetProperty("fields", out var userFields) &&
                            userFields.TryGetProperty("friendRequests", out var requestsField) &&
                            requestsField.TryGetProperty("arrayValue", out var arr) &&
                            arr.TryGetProperty("values", out var values))
                        {
                            foreach (var v in values.EnumerateArray())
                            {
                                if (v.TryGetProperty("stringValue", out var sv) &&
                                    sv.GetString() == _settingsService.Uid)
                                {
                                    result.FriendButtonText = "Request Sent";
                                    break;
                                }
                            }
                        }
                    }
                }

                Results.Add(result);
            }
        }
    }

    private async Task ToggleFriend(UserResult user)
    {
        if (user == null || string.IsNullOrEmpty(_settingsService.Uid)) return;

        if (user.FriendButtonText == "Add Friend")
        {
            var ok = await _friendService.SendFriendRequestAsync(_settingsService.Uid, user.Uid);
            if (ok) user.FriendButtonText = "Request Sent";
        }
        else if (user.FriendButtonText == "Remove Friend")
        {
            var ok = await _friendService.RemoveFriendAsync(_settingsService.Uid, user.Uid);
            if (ok) user.FriendButtonText = "Add Friend";
        }
        else if (user.FriendButtonText == "Request Sent")
        {
            var ok = await _friendService.RejectFriendRequestAsync(user.Uid, _settingsService.Uid);
            if (ok) user.FriendButtonText = "Add Friend";
        }
    }

    partial void OnSelectedUserChanged(UserResult? value)
    {
        if (value is null) return;

        _ = Shell.Current.GoToAsync(
            $"//SearchPage/{nameof(UserProfilePage)}",
            true,
            new Dictionary<string, object>
            {
                ["uid"] = value.Uid,
                ["stack"] = "search"
            });

        SelectedUser = null;
    }
}
