using System.Windows.Controls;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt.Views;

public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void Optimize_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        MemoryOptimizer.OptimizeAll();
    }

    private void AutoStart_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        OptimizeService.Start();
    }

    private void ToggleBoot_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        StartupManager.ToggleAutoStart();
    }
}
