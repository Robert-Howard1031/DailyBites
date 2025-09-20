using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;

namespace DailyBites.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private string _welcomeText;

    public HomeViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _welcomeText = $"Welcome, {_settingsService.Username} 👋";
    }

    [RelayCommand]
    private async Task Logout()
    {
        _settingsService.Logout();
        await Shell.Current.GoToAsync($"//LoginPage");
    }
}
