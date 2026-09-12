using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class OptimizePage : Page
{
    public OptimizePage()
    {
        InitializeComponent();
        DataContext = new OptimizeViewModel();
    }
}
