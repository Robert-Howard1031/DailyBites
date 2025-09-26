using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Models;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class RequestsViewModel : BaseViewModel
{
    private readonly IFriendService _friendService;
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    [ObservableProperty]
    private ObservableCollection<FriendRequest> _requests = new();

    public bool HasRequests => Requests.Count > 0;
    public bool HasNoRequests => Requests.Count == 0;

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand<string> AcceptCommand { get; }
    public IAsyncRelayCommand<string> RejectCommand { get; }

    public RequestsViewModel(
        IFriendService friendService,
        ISettingsService settingsService,
        IConfiguration config)
    {
        _friendService = friendService;
        _settingsService = settingsService;
        _config = config;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        LoadCommand.Execute(null);
        AcceptCommand = new AsyncRelayCommand<string>(AcceptAsync);
        RejectCommand = new AsyncRelayCommand<string>(RejectAsync);
    }

    public async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(_settingsService.Uid))
            return;

        IsBusy = true;
        try
        {
            var incoming = await _friendService.GetFriendRequestsAsync(_settingsService.Uid);

            Requests.Clear();
            foreach (var uid in incoming)
            {
                var user = await FetchUserAsync(uid);
                if (user != null)
                    Requests.Add(user);
            }

            OnPropertyChanged(nameof(HasRequests));
            OnPropertyChanged(nameof(HasNoRequests));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<FriendRequest?> FetchUserAsync(string uid)
    {
        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{uid}";

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        if (!json.RootElement.TryGetProperty("fields", out var fields)) return null;

        string GetString(string key) =>
            fields.TryGetProperty(key, out var f) && f.TryGetProperty("stringValue", out var sv)
                ? sv.GetString() ?? string.Empty
                : string.Empty;

        return new FriendRequest
        {
            Uid = uid,
            Username = GetString("username"),
            Name = GetString("name"),
            ProfilePicUrl = GetString("profilePicUrl")
        };
    }

    private async Task AcceptAsync(string requesterUid)
    {
        if (string.IsNullOrEmpty(requesterUid)) return;

        var success = await _friendService.AcceptFriendRequestAsync(_settingsService.Uid, requesterUid);
        if (success)
        {
            var item = Requests.FirstOrDefault(r => r.Uid == requesterUid);
            if (item != null) Requests.Remove(item);

            OnPropertyChanged(nameof(HasRequests));
            OnPropertyChanged(nameof(HasNoRequests));
        }
    }

    private async Task RejectAsync(string requesterUid)
    {
        if (string.IsNullOrEmpty(requesterUid)) return;

        var success = await _friendService.RejectFriendRequestAsync(_settingsService.Uid, requesterUid);
        if (success)
        {
            var item = Requests.FirstOrDefault(r => r.Uid == requesterUid);
            if (item != null) Requests.Remove(item);

            OnPropertyChanged(nameof(HasRequests));
            OnPropertyChanged(nameof(HasNoRequests));
        }
    }
}
