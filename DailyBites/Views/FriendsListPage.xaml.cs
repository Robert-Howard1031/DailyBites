using DailyBites.ViewModels;

namespace DailyBites.Views;

[QueryProperty(nameof(Uid), "uid")]
[QueryProperty(nameof(Stack), "stack")]
public partial class FriendsListPage : ContentPage
{
    private readonly FriendsListViewModel _vm;

    public string Uid
    {
        get => _vm.Uid;
        set => _vm.Uid = value;
    }
    public string Stack
    {
        get => _vm.Stack;
        set => _vm.Stack = value;
    }

    public FriendsListPage(FriendsListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is FriendsListViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
