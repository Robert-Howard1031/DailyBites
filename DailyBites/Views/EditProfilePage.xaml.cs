using DailyBites.ViewModels;

namespace DailyBites.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _vm;

    public EditProfilePage(EditProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is EditProfileViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
