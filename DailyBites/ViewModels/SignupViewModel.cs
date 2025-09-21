using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace DailyBites.ViewModels;

public partial class SignupViewModel : BaseViewModel
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _confirmPassword;

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

        // 🔹 Create Firebase Auth user
        var token = await _firebaseAuthService.SignupAsync(_email.Trim(), _password);
        if (token is null)
        {
            await Shell.Current.DisplayAlert("Signup Failed", "Please try again", "OK");
            return;
        }

        var projectId = _config["Firebase:ProjectId"];

        // 🔹 Save username + email to Firestore with Auth token
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users";
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            fields = new
            {
                username = new { stringValue = _username.Trim() },
                email = new { stringValue = _email.Trim() }
            }
        };

        var response = await _http.PostAsJsonAsync(url, body);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            await Shell.Current.DisplayAlert("Firestore Error", $"Could not save user: {error}", "OK");
            return;
        }

        // 🔹 Send email verification
        await _firebaseAuthService.SendVerificationEmailAsync(token);

        await Shell.Current.DisplayAlert("Verify Email",
            "We sent a verification email. Please verify, then log in.", "OK");

        await Shell.Current.GoToAsync($"//LoginPage");
    }

    [RelayCommand]
    private Task GoToLogin() => Shell.Current.GoToAsync($"//LoginPage");
}
