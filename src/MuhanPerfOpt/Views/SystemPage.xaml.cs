using System.Windows;
using System.Windows.Controls;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt.Views;

public partial class SystemPage : Page
{
    public SystemPage() => InitializeComponent();

    private void ToggleStart_Click(object sender, RoutedEventArgs e)
    {
        StartupManager.ToggleAutoStart();
        if (DataContext is ViewModels.SystemViewModel vm) vm.Refresh();
    }
}
