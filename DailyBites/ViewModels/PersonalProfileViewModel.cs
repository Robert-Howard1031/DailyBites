using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
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

    public PersonalProfileViewModel(IConfiguration config, ISettingsService settingsService)
    {
        _config = config;
        _settingsService = settingsService;

        // get the current logged in uid from settings
        _uid = _settingsService.Uid;

        // auto-load
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        LoadCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    private async Task LoadAsync()
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
            return fields.TryGetProperty(key, out var f) &&
                   f.TryGetProperty("stringValue", out var sv)
                ? sv.GetString() ?? string.Empty
                : string.Empty;
        }

        Username = GetString("username");
        Name = GetString("name");
        Email = GetString("email");
        ProfilePicUrl = GetString("profilePicUrl");

        if (fields.TryGetProperty("friends", out var friendsField) &&
            friendsField.TryGetProperty("arrayValue", out var arr) &&
            arr.TryGetProperty("values", out var vals))
        {
            FriendCount = vals.GetArrayLength();
        }
        else
        {
            FriendCount = 0;
        }
    }
}
