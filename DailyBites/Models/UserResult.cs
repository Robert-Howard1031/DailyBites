using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBites.Models;

public partial class UserResult : ObservableObject
{
    public string Uid { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfilePicUrl { get; set; } = string.Empty;

    [ObservableProperty]
    private string? _friendButtonText = "Add Friend";
}
