using Microsoft.UI.Xaml.Controls;
using MuhanPerfOpt.ViewModels;

namespace MuhanPerfOpt.Views;

public sealed partial class StoragePage : Page
{
    private readonly StorageViewModel _vm;

    public StoragePage()
    {
        InitializeComponent();
        _vm = new StorageViewModel();
        DataContext = _vm;
    }
}
