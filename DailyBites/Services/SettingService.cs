using Microsoft.Maui.Storage;

namespace DailyBites.Services;

public class SettingsService : ISettingsService
{
    private const string LoggedInKey = "IsLoggedIn";
    private const string EmailKey = "UserEmail";
    private const string UsernameKey = "Username";

    public bool IsLoggedIn
    {
        get => Preferences.Get(LoggedInKey, false);
        set => Preferences.Set(LoggedInKey, value);
    }

    public string UserEmail
    {
        get => Preferences.Get(EmailKey, string.Empty);
        set => Preferences.Set(EmailKey, value);
    }

    public string Username
    {
        get => Preferences.Get(UsernameKey, string.Empty);
        set => Preferences.Set(UsernameKey, value);
    }

    public void Logout()
    {
        Preferences.Remove(LoggedInKey);
        Preferences.Remove(EmailKey);
        Preferences.Remove(UsernameKey);
    }
}
