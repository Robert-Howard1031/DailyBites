using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace DailyBites.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    [ObservableProperty] private string _identifier; // Username OR Email
    [ObservableProperty] private string _password;

    public LoginViewModel(
        IFirebaseAuthService firebaseAuthService,
        ISettingsService settingsService,
        IConfiguration config)
    {
        _firebaseAuthService = firebaseAuthService;
        _settingsService = settingsService;
        _config = config;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(_identifier) || string.IsNullOrWhiteSpace(_password))
        {
            await Shell.Current.DisplayAlert("Error", "Username/Email and password are required", "OK");
            return;
        }

        string emailToUse;
        string usernameToUse;

        if (_identifier.Contains("@"))
        {
            // 🔹 User typed an email
            emailToUse = _identifier.Trim();

            // Look up username from Firestore
            usernameToUse = await LookupUsernameByEmail(emailToUse) ?? "";
        }
        else
        {
            // 🔹 User typed a username
            usernameToUse = _identifier.Trim();

            // Look up email from Firestore
            emailToUse = await LookupEmailByUsername(usernameToUse) ?? "";

            if (string.IsNullOrEmpty(emailToUse))
            {
                await Shell.Current.DisplayAlert("Login Failed", "No account found for that username.", "OK");
                return;
            }
        }

        var (idToken, verified) = await _firebaseAuthService.LoginAsync(emailToUse, _password);

        if (string.IsNullOrEmpty(idToken))
        {
            await Shell.Current.DisplayAlert("Login Failed", "Invalid credentials", "OK");
            return;
        }

        if (!verified)
        {
            await Shell.Current.DisplayAlert("Verify Email", "Please verify your email before logging in.", "OK");
            return;
        }

        // 🔹 Save correct values
        _settingsService.IsLoggedIn = true;
        _settingsService.UserEmail = emailToUse;
        _settingsService.Username = usernameToUse;

        await Shell.Current.GoToAsync($"//HomePage");
    }

    private async Task<string?> LookupEmailByUsername(string username)
    {
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
                }
            }
        };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return null;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());

        foreach (var element in json.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("document", out var doc) &&
                doc.TryGetProperty("fields", out var fields) &&
                fields.TryGetProperty("email", out var emailField))
            {
                return emailField.GetProperty("stringValue").GetString();
            }
        }

        return null;
    }

    private async Task<string?> LookupUsernameByEmail(string email)
    {
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
                        field = new { fieldPath = "email" },
                        op = "EQUAL",
                        value = new { stringValue = email }
                    }
                }
            }
        };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return null;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());

        foreach (var element in json.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("document", out var doc) &&
                doc.TryGetProperty("fields", out var fields) &&
                fields.TryGetProperty("username", out var usernameField))
            {
                return usernameField.GetProperty("stringValue").GetString();
            }
        }

        return null;
    }

    [RelayCommand]
    private Task GoToSignup() => Shell.Current.GoToAsync($"//SignupPage");
}
