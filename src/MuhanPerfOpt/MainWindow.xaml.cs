using Microsoft.UI.Xaml;

namespace MuhanPerfOpt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavView.SelectedItem = NavView.MenuItems[0];
    }
}
