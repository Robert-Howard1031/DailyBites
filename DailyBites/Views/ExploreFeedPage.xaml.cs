using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class ExploreFeedView : ContentView
{
    public ExploreFeedView(ExploreFeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
