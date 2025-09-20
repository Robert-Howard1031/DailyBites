namespace DailyBites.Services;

public interface IFirebaseAuthService
{
    Task<string?> SignupAsync(string email, string password);
    Task<(string? IdToken, bool Verified)> LoginAsync(string email, string password);
    Task SendVerificationEmailAsync(string idToken);
}
