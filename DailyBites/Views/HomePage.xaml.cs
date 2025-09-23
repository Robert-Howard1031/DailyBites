using DailyBites.ViewModels;
using Microsoft.Maui.Controls;

namespace DailyBites.Views;

public partial class HomePage : ContentPage
{
    private readonly FriendFeedView _friendFeed;
    private readonly ExploreFeedView _exploreFeed;

    public HomePage(FriendFeedView friendFeed, ExploreFeedView exploreFeed)
    {
        InitializeComponent();

        _friendFeed = friendFeed;
        _exploreFeed = exploreFeed;

        // Default to Friends tab
        FeedHost.Content = _friendFeed;
        SetActiveTab(FriendFeedButton, ExploreFeedButton);
    }

    private void OnFriendClicked(object sender, EventArgs e)
    {
        FeedHost.Content = _friendFeed;
        SetActiveTab(FriendFeedButton, ExploreFeedButton);
    }

    private void OnExploreClicked(object sender, EventArgs e)
    {
        FeedHost.Content = _exploreFeed;
        SetActiveTab(ExploreFeedButton, FriendFeedButton);
    }

    private void SetActiveTab(Button active, Button inactive)
    {
        VisualStateManager.GoToState(active, "Selected");
        VisualStateManager.GoToState(inactive, "Normal");
    }
}
