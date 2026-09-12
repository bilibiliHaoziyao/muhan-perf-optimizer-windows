using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class ScreenPage : Page
{
    private readonly ScreenViewModel _vm;

    public ScreenPage()
    {
        InitializeComponent();
        _vm = new ScreenViewModel();
        DataContext = _vm;
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _vm.Refresh();
    }
}
