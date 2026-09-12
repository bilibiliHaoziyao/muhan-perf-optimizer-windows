using System;
using Microsoft.Win32;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 开机自启动。使用 HKCU Run 注册表键（不需要管理员权限）。
/// </summary>
public static class StartupManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "MuhanPerfOpt";

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey);
                return key?.GetValue(AppName) != null;
            }
            catch { return false; }
        }
    }

    public static bool Enable()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKey, true);
            if (key == null) return false;
            var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                ?? Environment.ProcessPath ?? "MuhanPerfOpt.exe";
            key.SetValue(AppName, $"\"{exe}\" --silent", RegistryValueKind.String);
            return true;
        }
        catch { return false; }
    }

    public static bool Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            key?.DeleteValue(AppName, false);
            return true;
        }
        catch { return false; }
    }

    public static void Toggle()
    {
        if (IsEnabled) Disable();
        else Enable();
    }
}
