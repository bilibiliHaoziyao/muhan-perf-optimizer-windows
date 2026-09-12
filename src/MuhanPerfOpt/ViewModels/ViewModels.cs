using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Forms;
using System.Windows.Threading;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt.ViewModels;

/// <summary>
/// 所有页面共用的数据上下文。用 DispatcherTimer 保证 UI 线程更新。
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherTimer _timer;

    // ========== 公共属性 ==========
    public float CpuUsage { get; private set; }
    public string CpuUsageText => $"{CpuUsage:F0}%";
    public float CpuTemp { get; private set; }
    public string CpuTempText => CpuTemp > 0 ? $"{CpuTemp:F0} °C" : "-";
    public string CpuName { get; private set; } = "-";
    public int CpuCores { get; private set; } = Environment.ProcessorCount;

    public float GpuUsage { get; private set; }
    public string GpuUsageText => $"{GpuUsage:F0}%";
    public float GpuTemp { get; private set; }
    public string GpuTempText => GpuTemp > 0 ? $"{GpuTemp:F0} °C" : "-";
    public string GpuName { get; private set; } = "-";
    public string GpuMemory { get; private set; } = "-";
    public string GpuDedicatedMemory { get; private set; } = "-";

    public float MemoryPercent { get; private set; }
    public string MemoryText { get; private set; } = "-";
    public string MemoryAvailable { get; private set; } = "-";
    public long TotalRamGb { get; private set; }

    public float BatteryPercent { get; private set; } = -1;
    public string BatteryText => BatteryPercent < 0 ? "No Battery" : $"{BatteryPercent:F0}%";

    // Storage
    public ObservableCollection<DriveInfoViewModel> Drives { get; } = new();

    // Display
    public string MonitorCount { get; private set; } = "-";
    public string PrimaryResolution { get; private set; } = "-";
    public string TotalPixels { get; private set; } = "-";

    // System
    public string OsName { get; private set; } = Environment.OSVersion.ToString();
    public string OsVersion { get; private set; } = "-";
    public string Architecture { get; private set; } = Environment.Is64BitOperatingSystem ? "x64" : "x86";

    // Optimize
    public ObservableCollection<ProcessViewModel> Processes { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Refresh();
        _timer.Start();
        Refresh();
    }

    private void Refresh()
    {
        var m = HardwareMonitor.Shared;
        m.EnsureStarted();

        CpuUsage = m.CpuUsagePercent;
        CpuTemp = m.CpuTempC;
        CpuName = m.CpuName;
        GpuUsage = m.GpuUsagePercent;
        GpuTemp = m.GpuTempC;
        GpuName = m.GpuName;
        GpuMemory = $"{m.GpuMemoryUsedMb:F0} MB / {m.GpuMemoryTotalMb:F0} MB";

        MemoryPercent = m.RamPercent;
        var usedGb = m.RamUsedGb;
        var totalGb = m.RamTotalGb;
        TotalRamGb = (long)totalGb;
        MemoryText = $"{usedGb:F1} / {totalGb:F1} GB";
        MemoryAvailable = $"{totalGb - usedGb:F1} GB Free";

        BatteryPercent = m.BatteryPercent;

        // Storage
        RefreshStorage();
        // Display
        RefreshDisplay();
        // Processes
        RefreshProcesses();
        // System
        RefreshSystem();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    private void RefreshStorage()
    {
        try
        {
            var drives = DriveInfo.GetDrives();
            Drives.Clear();
            foreach (var d in drives)
            {
                if (!d.IsReady || d.DriveType == DriveType.Network || d.DriveType == DriveType.Ram) continue;
                Drives.Add(new DriveInfoViewModel
                {
                    Name = d.Name,
                    Label = d.VolumeLabel,
                    Type = d.DriveType.ToString(),
                    UsedBytes = d.TotalSize - d.AvailableFreeSpace,
                    TotalBytes = d.TotalSize,
                    UsedPercent = d.TotalSize > 0 ? (int)((d.TotalSize - d.AvailableFreeSpace) * 100 / d.TotalSize) : 0,
                    UsageText = $"{FormatBytes(d.TotalSize - d.AvailableFreeSpace)} / {FormatBytes(d.TotalSize)}"
                });
            }
        }
        catch { }
    }

    private void RefreshDisplay()
    {
        try
        {
            var screens = Screen.AllScreens;
            MonitorCount = $"{screens.Length} monitor(s)";
            if (screens.Length > 0)
            {
                var primary = Screen.PrimaryScreen;
                PrimaryResolution = $"{primary!.Bounds.Width} x {primary.Bounds.Height}";
                long totalPixels = 0;
                foreach (var s in screens) totalPixels += (long)s.Bounds.Width * s.Bounds.Height;
                TotalPixels = $"{totalPixels:N0} total pixels";
            }
        }
        catch { }
    }

    private void RefreshProcesses()
    {
        try
        {
            var ps = Process.GetProcesses();
            Processes.Clear();
            foreach (var p in ps)
            {
                try
                {
                    Processes.Add(new ProcessViewModel
                    {
                        Name = p.ProcessName,
                        MemoryMB = (long)(p.WorkingSet64 / 1024 / 1024),
                        CpuPercent = 0
                    });
                }
                catch { }
            }
            var sorted = new ObservableCollection<ProcessViewModel>();
            foreach (var p in Processes.OrderByDescending(x => x.MemoryMB).Take(50)) sorted.Add(p);
            Processes.Clear();
            foreach (var p in sorted) Processes.Add(p);
        }
        catch { }
    }

    private void RefreshSystem()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject mo in searcher.Get())
            {
                OsName = $"{mo["Caption"]}";
                OsVersion = $"{mo["Version"]} ({mo["BuildNumber"]})";
            }
        }
        catch { }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1) { order++; size /= 1024; }
        return $"{size:0.##} {sizes[order]}";
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}

public class DriveInfoViewModel
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public long UsedBytes { get; set; }
    public long TotalBytes { get; set; }
    public int UsedPercent { get; set; }
    public string UsageText { get; set; } = "";
}

public class ProcessViewModel
{
    public string Name { get; set; } = "";
    public long MemoryMB { get; set; }
    public float CpuPercent { get; set; }
}

/// <summary>
/// 为每个页面提供独立的 ViewModel（共享同一个 MainViewModel 数据源）。
/// </summary>
public class HomeViewModel : MainViewModel { }
public class SocViewModel : MainViewModel { }
public class OptimizeViewModel : MainViewModel { }
public class StorageViewModel : MainViewModel { }
public class ScreenViewModel : MainViewModel { }
public class SystemViewModel : MainViewModel
{
    public string AutoStartStatus => StartupManager.IsEnabled ? "Auto-start is ENABLED" : "Auto-start is DISABLED";
    public string AutoStartButtonText => StartupManager.IsEnabled ? "Disable Auto-Start" : "Enable Auto-Start";
    public void Refresh() => OnPropertyChanged(nameof(AutoStartStatus));
    protected new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
