using System;
using System.Drawing;
using System.Windows.Forms;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 系统托盘服务（WPF 兼容）。
/// 简化实现 - 仅提供托盘图标基本功能。
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private NotifyIcon? _tray;
    private readonly object _lock = new();

    public void Show(string text)
    {
        lock (_lock)
        {
            if (_tray != null) return;

            _tray = new NotifyIcon
            {
                Visible = true,
                Text = text,
                Icon = SystemIcons.Application
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Show", null, (s, e) => ShowMainWindow());
            menu.Items.Add("Optimize Now", null, (s, e) => OptimizeNow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => ExitApp());
            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += (s, e) => ShowMainWindow();
        }
    }

    private static void ShowMainWindow()
    {
        foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
        {
            w.Show();
            w.Activate();
            return;
        }
    }

    private static void OptimizeNow()
    {
        try { MemoryOptimizer.OptimizeAll(); }
        catch { }
    }

    private static void ExitApp()
    {
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
        }
    }
}
