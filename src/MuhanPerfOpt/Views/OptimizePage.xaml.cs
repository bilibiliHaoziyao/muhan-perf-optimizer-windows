using System.Windows;
using System.Windows.Controls;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt.Views;

public partial class OptimizePage : Page
{
    public OptimizePage() => InitializeComponent();

    private void Optimize_Click(object sender, RoutedEventArgs e)
    {
        var result = MemoryOptimizer.OptimizeAll();
        ToastService.Show("Optimized", $"Tried {result.Attempted}, cleaned {result.Succeeded} processes");
    }

    private void OptimizeSelected_Click(object sender, RoutedEventArgs e)
    {
        MemoryOptimizer.OptimizeAll();
    }

    private void StartAuto_Click(object sender, RoutedEventArgs e) => OptimizeService.Start();
    private void StopAuto_Click(object sender, RoutedEventArgs e) => OptimizeService.Stop();
}
