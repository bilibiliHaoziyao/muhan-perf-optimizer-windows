using Microsoft.UI.Xaml;

namespace MuhanPerfOpt;

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow w = new();
        w.Activate();
    }
}
