using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBites.Services;

namespace DailyBites.ViewModels;

public partial class ExploreFeedViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private string _welcomeText;

    public ExploreFeedViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _welcomeText = $"Welcome, to the explore feed";
    }

    [RelayCommand]
    private async Task Logout()
    {
        _settingsService.Logout();
        await Shell.Current.GoToAsync($"//LoginPage");
    }
}
