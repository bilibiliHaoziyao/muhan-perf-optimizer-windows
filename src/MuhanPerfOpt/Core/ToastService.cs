using System;

namespace MuhanPerfOpt.Core;

/// <summary>
/// Toast 通知服务（WPF + HandyControl 版本）。
/// </summary>
public static class ToastService
{
    public static void Show(string title, string message)
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HandyControl.Controls.Growl.InfoGlobal($"{title}: {message}");
            });
        }
        catch { }
    }

    public static void ShowSuccess(string message) => Show("Success", message);
    public static void ShowWarning(string message) => Show("Warning", message);
    public static void ShowError(string message) => Show("Error", message);
}
