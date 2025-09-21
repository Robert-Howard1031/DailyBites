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

        builder.Services.AddSingleton<SignupPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<HomePage>();

        return builder.Build();
    }
}
