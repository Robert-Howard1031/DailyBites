using DailyBites.Services;

namespace DailyBites;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

        private readonly ISettingsService _settingsService;
    public App(IServiceProvider serviceProvider)
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception e)
        {
            var ex = e.InnerException?.Message;
        }
        ServiceProvider = serviceProvider;
        _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
    protected override async void OnStart()
    {
        base.OnStart();

        // TODO: When the main page is created, uncomment the code below to navigate to the main page if the user is logged in.
        bool isUserLoggedIn = _settingsService.IsLoggedIn;
        var page = isUserLoggedIn ? "//HomePage" : "//LoginPage";

        //var page = "//LoginPage";
        await Shell.Current.GoToAsync(page);
    }
}
