using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DailyBites.Services;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly HttpClient _http = new();
    private readonly string _apiKey;

    public FirebaseAuthService(IConfiguration config)
    {
        _apiKey = config["Firebase:ApiKey"]
                  ?? throw new Exception("Firebase ApiKey missing in appsettings.json");
    }

    public async Task<string?> SignupAsync(string email, string password)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
        var body = new { email, password, returnSecureToken = true };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return null;

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        return json.RootElement.TryGetProperty("idToken", out var token)
            ? token.GetString()
            : null;
    }

    public async Task<(string? IdToken, bool Verified)> LoginAsync(string email, string password)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
        var body = new { email, password, returnSecureToken = true };

        var res = await _http.PostAsJsonAsync(url, body);
        if (!res.IsSuccessStatusCode) return (null, false);

        using var json = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());

        string? token = json.RootElement.TryGetProperty("idToken", out var tokenProp)
            ? tokenProp.GetString()
            : null;

        bool verified = json.RootElement.TryGetProperty("emailVerified", out var verifiedProp)
            && verifiedProp.GetBoolean();

        // 🔹 If emailVerified missing or false, fetch user info explicitly
        if (!verified && !string.IsNullOrEmpty(token))
        {
            var infoUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";
            var infoBody = new { idToken = token };
            var infoRes = await _http.PostAsJsonAsync(infoUrl, infoBody);

            if (infoRes.IsSuccessStatusCode)
            {
                using var infoJson = await JsonDocument.ParseAsync(await infoRes.Content.ReadAsStreamAsync());
                if (infoJson.RootElement.TryGetProperty("users", out var usersArr) &&
                    usersArr[0].TryGetProperty("emailVerified", out var verifiedField))
                {
                    verified = verifiedField.GetBoolean();
                }
            }
        }

        return (token, verified);
    }


    public async Task SendVerificationEmailAsync(string idToken)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
        var body = new { requestType = "VERIFY_EMAIL", idToken };
        await _http.PostAsJsonAsync(url, body);
    }
}
