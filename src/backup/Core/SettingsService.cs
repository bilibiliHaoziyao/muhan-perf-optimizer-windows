using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 用户设置持久化（JSON 文件放在 AppData/Local/MuhanPerfOpt/config.json）。
/// 对应 Android 端 Prefs / SharedPreferences。
/// </summary>
public static class SettingsService
{
    public static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MuhanPerfOpt", "config.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { Current = new AppSettings(); }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static void IncrementCleanStat() => Current.TotalCleanCount++;
}

public class AppSettings
{
    /// <summary>主题：light / dark / system</summary>
    public string Theme { get; set; } = "system";

    /// <summary>是否启用后台自动清理</summary>
    public bool AutoCleanEnabled { get; set; } = true;

    /// <summary>RAM 占用超过该阈值自动清理（%）</summary>
    public int CleanThresholdPercent { get; set; } = 85;

    /// <summary>白名单：这些进程不会被清理</summary>
    public HashSet<string> Whitelist { get; set; } = new() { "devenv", "code", "discord", "obs64" };

    /// <summary>开机自启</summary>
    public bool AutoStartEnabled { get; set; } = false;

    /// <summary>总清理次数</summary>
    public int TotalCleanCount { get; set; } = 0;

    /// <summary>是否显示 Toast 通知</summary>
    public bool ShowToastOnClean { get; set; } = true;
}
