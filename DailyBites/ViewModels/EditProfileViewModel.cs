using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Maui.Media; 
using Microsoft.Maui.Storage; 

namespace DailyBites.ViewModels;

public partial class EditProfileViewModel : BaseViewModel
{
    private readonly IConfiguration _config;
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _http = new();

    private string? _originalUsername;

    [ObservableProperty] 
    private string _uid;
    [ObservableProperty] 
    private string _username = string.Empty;
    [ObservableProperty] 
    private string _name = string.Empty;
    [ObservableProperty] 
    private string _bio = string.Empty;
    [ObservableProperty] 
    private string _profilePicUrl = string.Empty;

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand UploadPhotoCommand { get; }
    public IAsyncRelayCommand TakePhotoCommand { get; }

    public EditProfileViewModel(IConfiguration config, ISettingsService settingsService)
    {
        _config = config;
        _settingsService = settingsService;
        _uid = _settingsService.Uid;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        UploadPhotoCommand = new AsyncRelayCommand(UploadPhotoAsync);
        TakePhotoCommand = new AsyncRelayCommand(TakePhotoAsync);
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

        string GetString(string key) =>
            fields.TryGetProperty(key, out var f) &&
            f.TryGetProperty("stringValue", out var sv)
                ? sv.GetString() ?? string.Empty
                : string.Empty;

        Username = GetString("username");
        Name = GetString("name");
        Bio = GetString("bio");
        ProfilePicUrl = GetString("profilePicUrl");

        _originalUsername = Username;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Uid)) return;

        // Only check if username changed
        if (!string.Equals(Username, _originalUsername, StringComparison.OrdinalIgnoreCase))
        {
            var available = await IsUsernameAvailableAsync(Username);
            if (!available)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Username already taken", "OK");
                return;
            }
        }

        var projectId = _config["Firebase:ProjectId"];
        var url =
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{Uid}" +
            $"?updateMask.fieldPaths=username&updateMask.fieldPaths=name&updateMask.fieldPaths=bio&updateMask.fieldPaths=profilePicUrl";

        var body = new
        {
            fields = new
            {
                username = new { stringValue = Username ?? string.Empty },
                name = new { stringValue = Name ?? string.Empty },
                bio = new { stringValue = Bio ?? string.Empty },
                profilePicUrl = new { stringValue = ProfilePicUrl ?? string.Empty }
            }
        };

        var res = await _http.PatchAsJsonAsync(url, body);
        if (res.IsSuccessStatusCode)
        {
            _originalUsername = Username;
            await Application.Current.MainPage.DisplayAlert("Success", "Profile updated!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            var err = await res.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Error", $"Update failed:\n{err}", "OK");
        }
    }

    private async Task<bool> IsUsernameAvailableAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        var projectId = _config["Firebase:ProjectId"];
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

        var body = new
        {
            structuredQuery = new
            {
                from = new[] { new { collectionId = "users" } },
                where = new
                {
                    fieldFilter = new
                    {
                        field = new { fieldPath = "username" },
                        op = "EQUAL",
                        value = new { stringValue = username }
                    }
                },
                limit = 1
            }
        };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return false;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        return !json.RootElement.EnumerateArray().Any(e => e.TryGetProperty("document", out _));
    }

    private async Task UploadPhotoAsync()
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a profile picture",
                FileTypes = FilePickerFileType.Images
            });

            if (file == null) return;
            using var stream = await file.OpenReadAsync();
            await UploadToFirebaseAsync(stream);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Photo upload failed: {ex.Message}", "OK");
        }
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Camera not supported on this device", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take profile picture"
            });

            if (photo == null) return;
            using var stream = await photo.OpenReadAsync();
            await UploadToFirebaseAsync(stream);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Camera failed: {ex.Message}", "OK");
        }
    }

    private async Task UploadToFirebaseAsync(Stream fileStream)
    {
        var bucket = "dailybites-ca068.firebasestorage.app";
        var objectPath = $"profilePics/{Uid}.jpg";
        var url = $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o?uploadType=media&name={Uri.EscapeDataString(objectPath)}";

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var res = await _http.PostAsync(url, content);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Error", $"Upload failed:\n{res.StatusCode}\n{err}", "OK");
            return;
        }

        //  Add a cache-busting query (timestamp)
        var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var downloadUrl =
            $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString(objectPath)}?alt=media&cb={cacheBuster}";

        // Update local binding
        ProfilePicUrl = downloadUrl;

        // Update Firestore immediately
        var projectId = _config["Firebase:ProjectId"];
        var docUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{Uid}?updateMask.fieldPaths=profilePicUrl";

        var body = new
        {
            fields = new
            {
                profilePicUrl = new { stringValue = ProfilePicUrl }
            }
        };

        var updateRes = await _http.PatchAsJsonAsync(docUrl, body);
        if (!updateRes.IsSuccessStatusCode)
        {
            var err = await updateRes.Content.ReadAsStringAsync();
            await Application.Current.MainPage.DisplayAlert("Error", $"ProfilePicUrl Firestore update failed:\n{err}", "OK");
            return;
        }

        await Application.Current.MainPage.DisplayAlert("Success", "Profile picture updated!", "OK");
    }



}
