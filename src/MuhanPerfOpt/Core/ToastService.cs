using System;
using System.Windows;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 简单 Toast 通知服务（WPF MessageBox + 写入调试日志）。
/// 简化实现 - 后续可升级为自定义弹窗。
/// </summary>
public static class ToastService
{
    public static void Show(string title, string message)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[{title}] {message}");
        }
        catch { }
    }

    public static void ShowSuccess(string message) => Show("Success", message);
    public static void ShowWarning(string message) => Show("Warning", message);
    public static void ShowError(string message) => Show("Error", message);
}
