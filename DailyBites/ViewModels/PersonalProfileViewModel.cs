using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using DailyBites.Views;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class PersonalProfileViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _http = new();

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
    private string _bio = string.Empty;

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand ViewFriendsCommand { get; }
    public IAsyncRelayCommand EditProfileCommand { get; }

    public PersonalProfileViewModel(IConfiguration config, ISettingsService settingsService)
    {
        _config = config;
        _settingsService = settingsService;

        _uid = _settingsService.Uid;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        ViewFriendsCommand = new AsyncRelayCommand(OnViewFriendsClicked);
        EditProfileCommand = new AsyncRelayCommand(OnEditProfileClicked);

        LoadCommand.Execute(null);
    }

    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(Uid)) return;

        try
        {
            IsBusy = true;

            var projectId = _config["Firebase:ProjectId"];
            var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{Uid}";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return;

            using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            if (!json.RootElement.TryGetProperty("fields", out var fields)) return;

            string GetString(string key) =>
                fields.TryGetProperty(key, out var f) &&
                f.TryGetProperty("stringValue", out var sv)
                    ? sv.GetString() ?? string.Empty
                    : string.Empty;

            Username = GetString("username");
            Name = GetString("name");
            Email = GetString("email");
            ProfilePicUrl = GetString("profilePicUrl");
            Bio = GetString("bio");

            if (fields.TryGetProperty("friends", out var friendsField) &&
                friendsField.TryGetProperty("arrayValue", out var arr) &&
                arr.TryGetProperty("values", out var vals))
            {
                FriendCount = vals.EnumerateArray().Count();
            }
            else
            {
                FriendCount = 0;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnViewFriendsClicked()
    {
        await Shell.Current.GoToAsync(
            $"//PersonalProfilePage/{nameof(FriendsListPage)}",
            true,
            new Dictionary<string, object>
            {
                ["uid"] = Uid,
                ["stack"] = "personal",
            }
        );
    }

    private async Task OnEditProfileClicked()
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }
}
