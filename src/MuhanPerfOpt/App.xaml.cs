using System;
using Microsoft.UI.Xaml;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"Fatal: {e.Message}\n{e.Exception}");
            e.Handled = true;
        };
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // 初始化服务层
        HardwareMonitor.Shared.Start();
        SettingsService.Load();

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    protected override void OnExit(Windows.ApplicationModel.ExitApplicationEventArgs args)
    {
        HardwareMonitor.Shared.Stop();
        SettingsService.Save();
        base.OnExit(args);
    }
}
