using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class FriendFeedView : ContentView
{
    public FriendFeedView(FriendFeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
