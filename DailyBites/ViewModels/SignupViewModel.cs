using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace DailyBites.ViewModels;

public partial class SignupViewModel : BaseViewModel
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    [ObservableProperty]
    private string _username;
    [ObservableProperty]
    private string _email;
    [ObservableProperty]
    private string _password;
    [ObservableProperty]
    private string _confirmPassword;

    public SignupViewModel(IFirebaseAuthService firebaseAuthService, IConfiguration config)
    {
        _firebaseAuthService = firebaseAuthService;
        _config = config;
    }

    [RelayCommand]
    private async Task Signup()
    {
        if (string.IsNullOrWhiteSpace(_username) ||
            string.IsNullOrWhiteSpace(_email) ||
            string.IsNullOrWhiteSpace(_password))
        {
            await Shell.Current.DisplayAlert("Error", "All fields are required", "OK");
            return;
        }

        if (_password != _confirmPassword)
        {
            await Shell.Current.DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        // 🔹 Normalize username to lowercase
        var lowerUsername = _username.Trim().ToLower();

        // 🔹 Validate allowed characters
        if (!Regex.IsMatch(lowerUsername, @"^[a-z0-9._-]+$"))
        {
            await Shell.Current.DisplayAlert("Error",
                "Username can only contain letters, numbers, underscores (_), hyphens (-), or dots (.)",
                "OK");
            return;
        }

        var projectId = _config["Firebase:ProjectId"];

        //  Check if username already exists
        var queryUrl =
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

        var queryBody = new
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
                        value = new { stringValue = lowerUsername }
                    }
                }
            }
        };

        var queryResponse = await _http.PostAsJsonAsync(queryUrl, queryBody);
        var queryContent = await queryResponse.Content.ReadAsStringAsync();

        if (queryContent.Contains("fields"))
        {
            await Shell.Current.DisplayAlert("Error", "Username already taken", "OK");
            return;
        }

        // Create Firebase Auth user
        var token = await _firebaseAuthService.SignupAsync(_email.Trim(), _password);
        if (token is null)
        {
            await Shell.Current.DisplayAlert("Signup Failed", "Please try again", "OK");
            return;
        }

        //  Extract UID from JWT
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var uid = jwt.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

        if (string.IsNullOrEmpty(uid))
        {
            await Shell.Current.DisplayAlert("Error", "Could not get user ID", "OK");
            return;
        }

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //  Pull default profile picture from config
        var defaultPic = _config["Firebase:DefaultProfilePicUrl"] ?? string.Empty;

        //  Save user to Firestore
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{uid}";
        var body = new
        {
            fields = new
            {
                uid = new { stringValue = uid },
                username = new { stringValue = lowerUsername },
                email = new { stringValue = _email.Trim() },
                profilePicUrl = new { stringValue = defaultPic } // ✅ default picture
            }
        };

        var response = await _http.PatchAsJsonAsync(url, body);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await Shell.Current.DisplayAlert("Firestore Error", $"Could not save user: {error}", "OK");
            return;
        }

        //  Send email verification
        await _firebaseAuthService.SendVerificationEmailAsync(token);

        await Shell.Current.DisplayAlert("Verify Email",
            "We sent a verification email. Please verify, then log in.", "OK");

        await Shell.Current.GoToAsync($"//LoginPage");
    }

    [RelayCommand]
    private Task GoToLogin() => Shell.Current.GoToAsync($"//LoginPage");
}
