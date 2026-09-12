using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class SocPage : Page
{
    private readonly SocViewModel _vm;

    public SocPage()
    {
        InitializeComponent();
        _vm = new SocViewModel();
        DataContext = _vm;
    }
}
