using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class OverviewPage : Page
{
    private readonly OverviewViewModel _vm;
    public OverviewPage()
    {
        InitializeComponent();
        _vm = new OverviewViewModel();
        DataContext = _vm;
    }

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        _vm.Stop();
        base.OnNavigatedFrom(e);
    }
}
