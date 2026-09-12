using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class SystemPage : Page
{
    private readonly SystemViewModel _vm;

    public SystemPage()
    {
        InitializeComponent();
        _vm = new SystemViewModel();
        DataContext = _vm;
    }
}
