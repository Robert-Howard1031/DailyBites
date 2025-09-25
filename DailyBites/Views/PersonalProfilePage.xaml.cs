using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class PersonalProfilePage : ContentPage
{
    public PersonalProfilePage(PersonalProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
