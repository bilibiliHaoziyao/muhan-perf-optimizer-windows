using System;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhanPerfOpt.Core;
using MuhanPerfOpt.ViewModels;
using MuhanPerfOpt.Views;

namespace MuhanPerfOpt;

public sealed partial class MainWindow : Window
{
    private readonly OverviewViewModel _overviewVm = new();
    private readonly SocViewModel _socVm = new();
    private readonly OptimizeViewModel _optimizeVm = new();
    private readonly StorageViewModel _storageVm = new();
    private readonly ScreenViewModel _screenVm = new();
    private readonly SystemViewModel _systemVm = new();
    private readonly SettingsViewModel _settingsVm = new();

    public MainWindow()
    {
        InitializeComponent();
        Title = "慕寒性能优化";

        // Fluent 导航：侧边 NavigationView + Settings 页
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(OverviewPage), _overviewVm);

        NavView.ItemInvoked += OnNavItemInvoked;
        ContentFrame.NavigationFailed += OnNavigationFailed;
        Closed += OnWindowClosed;

        // 启动后台监控与系统托盘
        _ = StartBackgroundServicesAsync();
    }

    private void OnNavItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is not NavigationViewItem item) return;
        var tag = item.Tag as string ?? "";
        switch (tag)
        {
            case "overview":
                ContentFrame.Navigate(typeof(OverviewPage), _overviewVm); break;
            case "soc":
                ContentFrame.Navigate(typeof(SocPage), _socVm); break;
            case "optimize":
                ContentFrame.Navigate(typeof(OptimizePage), _optimizeVm); break;
            case "storage":
                ContentFrame.Navigate(typeof(StoragePage), _storageVm); break;
            case "screen":
                ContentFrame.Navigate(typeof(ScreenPage), _screenVm); break;
            case "system":
                ContentFrame.Navigate(typeof(SystemPage), _systemVm); break;
            case "settings":
                ContentFrame.Navigate(typeof(SettingsPage), _settingsVm); break;
        }
    }

    private void OnNavigationFailed(object? sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load Page {e.SourcePageType.FullName}");
    }

    private void OnWindowClosed(object? sender, WindowEventArgs args)
    {
        SettingsService.Save();
    }

    private async Task StartBackgroundServicesAsync()
    {
        // 1. 系统托盘（常驻）
        TrayIconService.Initialize(this);

        // 2. 若启用，后台自动清理
        if (SettingsService.Current.AutoCleanEnabled)
        {
            OptimizeService.StartAutoClean();
        }

        // 3. 开机自启状态校验
        if (SettingsService.Current.AutoStartEnabled)
        {
            StartupManager.Enable();
        }

        await Task.CompletedTask;
    }
}
