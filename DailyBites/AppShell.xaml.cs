using DailyBites.Views;
namespace DailyBites;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Explicit route registration
        Routing.RegisterRoute(nameof(SignupPage), typeof(SignupPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(FriendFeedPage), typeof(FriendFeedPage));
        Routing.RegisterRoute(nameof(ExploreFeedPage), typeof(ExploreFeedPage));
    }
}