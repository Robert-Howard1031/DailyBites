using DailyBites.ViewModels;
using Microsoft.Maui.Controls;

namespace DailyBites.Views;

[QueryProperty(nameof(Uid), "uid")]
public partial class UserProfilePage : ContentPage
{
    private readonly UserProfileViewModel _vm;

    public string Uid
    {
        get => _vm.Uid;
        set => _vm.Uid = value;
    }

    public UserProfilePage(UserProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
