using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MuhanPerfOpt.Core;

/// <summary>
/// Windows Toast 通知（通知中心常驻）。
/// 对应 Android 端 LiveUpdateService + Notification。
/// </summary>
public static class ToastService
{
    private const string AppId = "MuhanPerfOpt";
    private static ToastNotifier? _notifier;

    public static void EnsureRegistered()
    {
        try
        {
            _notifier = ToastNotificationManager.CreateToastNotifier(AppId);
            // 注册 AUMID（App User Model ID）用于关联到"慕寒性能优化"应用
            var tag = ToastNotificationManager.GetDefault();
        }
        catch { }
    }

    public static async Task ShowAsync(string title, string body)
    {
        if (!SettingsService.Current.ShowToastOnClean) return;
        try
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var texts = toastXml.GetElementsByTagName("text");
            texts[0].AppendChild(toastXml.CreateTextNode(title));
            texts[1].AppendChild(toastXml.CreateTextNode(body));

            var toast = new ToastNotification(toastXml);
            _notifier ??= ToastNotificationManager.CreateToastNotifier(AppId);
            _notifier!.Show(toast);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Toast 发送失败: {ex.Message}");
        }
    }

    public static void ShowOptimized(int count)
    {
        _ = ShowAsync("慕寒性能优化", $"已清理 {count} 个进程的工作集");
    }

    public static void ShowMonitorUpdate(string summary)
    {
        _ = ShowAsync("慕寒性能优化 - 实时", summary);
    }
}
