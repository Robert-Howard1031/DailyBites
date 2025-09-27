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
        Routing.RegisterRoute(nameof(FriendFeedView), typeof(FriendFeedView));
        Routing.RegisterRoute(nameof(ExploreFeedView), typeof(ExploreFeedView));
        Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
        Routing.RegisterRoute(nameof(PersonalProfilePage), typeof(PersonalProfilePage));
        Routing.RegisterRoute(nameof(RequestsPage), typeof(RequestsPage));
        Routing.RegisterRoute(nameof(FriendsListPage), typeof(FriendsListPage));
    }
}