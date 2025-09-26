using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class RequestsPage : ContentPage
{
    public RequestsPage(RequestsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
