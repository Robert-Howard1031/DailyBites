using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;

namespace DailyBites.ViewModels;

public partial class FriendFeedViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private string _welcomeText;

    public FriendFeedViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _welcomeText = $"Welcome, to the friend feed";
    }

    [RelayCommand]
    private async Task Logout()
    {
        _settingsService.Logout();
        await Shell.Current.GoToAsync($"//LoginPage");
    }
}
