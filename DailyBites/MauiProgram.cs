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

        builder.Services.AddSingleton<BaseViewModel>();
        builder.Services.AddSingleton<SignupViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<FriendFeedViewModel>();
        builder.Services.AddSingleton<ExploreFeedViewModel>();
        builder.Services.AddSingleton<SearchViewModel>();
        builder.Services.AddSingleton<UserProfileViewModel>();

        builder.Services.AddSingleton<SignupPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<FriendFeedView>();
        builder.Services.AddSingleton<ExploreFeedView>();
        builder.Services.AddSingleton<SearchPage>();
        builder.Services.AddSingleton<UserProfilePage>();

        return builder.Build();
    }
}
