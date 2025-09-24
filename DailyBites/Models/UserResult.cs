using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBites.Models;

public class UserResult : ObservableObject
{
    public string Uid { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfilePicUrl { get; set; } = string.Empty;
}
