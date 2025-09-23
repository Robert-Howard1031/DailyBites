using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DailyBites.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty] 
    private bool _isFriendSelected = true;
    [ObservableProperty] 
    private bool _isExploreSelected = false;

    public event Action<string>? FeedChanged; // "Friends" or "Explore"

    [RelayCommand]
    private void ShowFriends()
    {
        if (_isFriendSelected) return;
        _isFriendSelected = true;
        _isExploreSelected = false;
        FeedChanged?.Invoke("Friends");
    }

    [RelayCommand]
    private void ShowExplore()
    {
        if (_isExploreSelected) return;
        _isFriendSelected = false;
        _isExploreSelected = true;
        FeedChanged?.Invoke("Explore");
    }
}
