using DailyBites;
using DailyBites.Services;
using DailyBites.ViewModels;
using DailyBites.Views;
using Microsoft.Extensions.Configuration;
using System.Reflection;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
        builder.Configuration.AddJsonStream(stream);

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services/viewmodels/views
        builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IFriendService, FriendService>();


        builder.Services.AddTransient<BaseViewModel>();
        builder.Services.AddTransient<SignupViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<FriendFeedViewModel>();
        builder.Services.AddTransient<ExploreFeedViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<UserProfileViewModel>();
        builder.Services.AddTransient<PersonalProfileViewModel>();
        builder.Services.AddTransient<RequestsViewModel>();
        builder.Services.AddTransient<FriendsListViewModel>();

        builder.Services.AddTransient<SignupPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<FriendFeedView>();
        builder.Services.AddTransient<ExploreFeedView>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<UserProfilePage>();
        builder.Services.AddTransient<PersonalProfilePage>();
        builder.Services.AddTransient<RequestsPage>();
        builder.Services.AddTransient<FriendsListPage>();

        return builder.Build();
    }
}
