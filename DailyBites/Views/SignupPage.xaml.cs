using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class SignupPage : ContentPage
{
    public SignupPage(SignupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
