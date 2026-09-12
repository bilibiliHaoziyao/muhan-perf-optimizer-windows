using System.Windows;

namespace MuhanPerfOpt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => Navigate("Home");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string tag)
        {
            Navigate(tag);
        }
    }

    private void Navigate(string tag)
    {
        switch (tag)
        {
            case "Home": ContentFrame.Navigate(typeof(Views.HomePage)); break;
            case "Soc": ContentFrame.Navigate(typeof(Views.SocPage)); break;
            case "Optimize": ContentFrame.Navigate(typeof(Views.OptimizePage)); break;
            case "Storage": ContentFrame.Navigate(typeof(Views.StoragePage)); break;
            case "Screen": ContentFrame.Navigate(typeof(Views.ScreenPage)); break;
            case "System": ContentFrame.Navigate(typeof(Views.SystemPage)); break;
        }
    }
}
