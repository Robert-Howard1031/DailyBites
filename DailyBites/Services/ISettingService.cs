namespace DailyBites.Services;

public interface ISettingsService
{
    bool IsLoggedIn { get; set; }
    string UserEmail { get; set; }
    string Username { get; set; }
    string Uid { get; set; }
    void Logout();
}
