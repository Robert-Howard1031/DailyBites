using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class ExploreFeedPage : ContentPage
{
    public ExploreFeedPage(ExploreFeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
