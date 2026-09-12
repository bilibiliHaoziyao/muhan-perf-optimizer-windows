using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.UI.Xaml;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 系统托盘图标。常驻后台，双击/右键菜单打开主窗口或手动清理。
/// </summary>
public static class TrayIconService
{
    private static NotifyIcon? _tray;
    private static Window? _mainWindow;

    public static void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
        if (_tray != null) return;

        _tray = new NotifyIcon
        {
            Text = "慕寒性能优化",
            Visible = true,
            // 用系统自带图标替代，避免图标资源缺失导致崩溃
            Icon = SystemIcons.Shield
        };

        _tray.DoubleClick += (s, e) => ShowMain();
        _tray.ContextMenuStrip = BuildMenu();
    }

    private static ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("打开主界面", null, (s, e) => ShowMain());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("立即清理内存", null, (s, e) =>
        {
            var result = MemoryOptimizer.CleanProcesses();
            MemoryOptimizer.PurgeSystemFileCache();
            if (result.Succeeded > 0)
                ToastService.ShowOptimized(result.Succeeded);
        });
        menu.Items.Add("后台监控", null, (s, e) =>
        {
            if (OptimizeService.IsRunning) OptimizeService.Stop();
            else OptimizeService.StartAutoClean();
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (s, e) =>
        {
            OptimizeService.Stop();
            TrayIcon.Dispose();
            Environment.Exit(0);
        });
        return menu;
    }

    private static void ShowMain()
    {
        var w = _mainWindow;
        if (w == null) return;
        // 切到 UI 线程
        try
        {
            w.DispatcherQueue.TryEnqueue(() =>
            {
                w.Activate();
                w.AppWindow?.Show();
            });
        }
        catch { }
    }

    private static NotifyIcon TrayIcon { get => _tray!; }
}
