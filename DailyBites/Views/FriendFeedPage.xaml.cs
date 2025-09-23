using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class FriendFeedPage : ContentPage
{
    public FriendFeedPage(FriendFeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
