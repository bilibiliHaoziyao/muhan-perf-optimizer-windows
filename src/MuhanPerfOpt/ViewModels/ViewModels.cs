using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuhanPerfOpt.Core;

namespace MuhanPerfOpt.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    private readonly System.Threading.Timer _timer;

    [ObservableProperty] private float _cpuPercent;
    [ObservableProperty] private float _cpuTempC;
    [ObservableProperty] private string _cpuName = "—";
    [ObservableProperty] private float _gpuPercent;
    [ObservableProperty] private float _gpuTempC;
    [ObservableProperty] private string _gpuName = "—";
    [ObservableProperty] private long _ramUsedGb;
    [ObservableProperty] private long _ramTotalGb;
    [ObservableProperty] private float _ramPercent;
    [ObservableProperty] private float _batteryPercent = -1;
    [ObservableProperty] private float _batteryPowerW;
    [ObservableProperty] private string _uptime = "—";

    public OverviewViewModel()
    {
        _timer = new System.Threading.Timer(_ => Refresh(), null, 0, 1000);
    }

    private void Refresh()
    {
        try
        {
            var m = HardwareMonitor.Shared;
            CpuPercent = m.CpuUsagePercent;
            CpuTempC = m.CpuTempC;
            CpuName = m.CpuName;
            GpuPercent = m.GpuUsagePercent;
            GpuTempC = m.GpuTempC;
            GpuName = m.GpuName;
            RamTotalGb = m.RamTotalBytes / 1024 / 1024 / 1024;
            RamUsedGb = m.RamUsedBytes / 1024 / 1024 / 1024;
            RamPercent = m.RamTotalBytes > 0 ? (float)m.RamUsedBytes / m.RamTotalBytes * 100f : 0;
            BatteryPercent = m.BatteryPercent;
            BatteryPowerW = m.BatteryPowerWatts;
            Uptime = FormatUptime();
        }
        catch { }
    }

    private static string FormatUptime()
    {
        var uptime = Environment.TickCount64 / 1000;
        var days = uptime / 86400;
        var hrs = (uptime % 86400) / 3600;
        var mins = (uptime % 3600) / 60;
        return days > 0 ? $"{days}天 {hrs}时 {mins}分" : $"{hrs}时 {mins}分";
    }

    public void Stop() => _timer.Dispose();
}

public partial class OptimizeViewModel : ObservableObject
{
    [ObservableProperty] private int _cleanCount = SettingsService.Current.TotalCleanCount;
    [ObservableProperty] private bool _autoCleanEnabled = SettingsService.Current.AutoCleanEnabled;
    [ObservableProperty] private int _threshold = SettingsService.Current.CleanThresholdPercent;
    [ObservableProperty] private string _lastResult = "—";
    [ObservableProperty] private bool _isRunning = OptimizeService.IsRunning;

    public ICommand CleanNowCommand { get; }
    public ICommand ToggleAutoCommand { get; }
    public ICommand SaveThresholdCommand { get; }

    public OptimizeViewModel()
    {
        CleanNowCommand = new RelayCommand(async () =>
        {
            await Task.Run(() =>
            {
                var r = MemoryOptimizer.CleanProcesses();
                MemoryOptimizer.PurgeSystemFileCache();
                SettingsService.IncrementCleanStat();
                SettingsService.Save();
                LastResult = $"Cleaned {r.Succeeded} processes, skipped {r.Skipped}, failed {r.Failed}";
                CleanCount = SettingsService.Current.TotalCleanCount;
            });
        });

        ToggleAutoCommand = new RelayCommand(() =>
        {
            AutoCleanEnabled = !AutoCleanEnabled;
            SettingsService.Current.AutoCleanEnabled = AutoCleanEnabled;
            IsRunning = AutoCleanEnabled;
            if (AutoCleanEnabled) OptimizeService.StartAutoClean();
            else OptimizeService.Stop();
            SettingsService.Save();
        });

        SaveThresholdCommand = new RelayCommand(() =>
        {
            SettingsService.Current.CleanThresholdPercent = Threshold;
            SettingsService.Save();
        });
    }
}

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _autoStart = SettingsService.Current.AutoStartEnabled;
    [ObservableProperty] private bool _autoClean = SettingsService.Current.AutoCleanEnabled;
    [ObservableProperty] private bool _toastOnClean = SettingsService.Current.ShowToastOnClean;

    public ICommand SaveCommand { get; } = new RelayCommand(() =>
    {
        SettingsService.Current.AutoStartEnabled = AutoStart;
        SettingsService.Current.AutoCleanEnabled = AutoClean;
        SettingsService.Current.ShowToastOnClean = ToastOnClean;
        SettingsService.Save();
        if (AutoStart) StartupManager.Enable(); else StartupManager.Disable();
        if (AutoClean) OptimizeService.StartAutoClean(); else OptimizeService.Stop();
    });
}

public class SocViewModel : ObservableObject
{
    private readonly System.Threading.Timer _timer;
    public float CpuPercent { get; private set; }
    public float CpuTempC { get; private set; }
    public string CpuName { get; private set; } = "—";
    public float[] CoreUsages { get; private set; } = Array.Empty<float>();
    public float GpuPercent { get; private set; }
    public float GpuTempC { get; private set; }
    public float GpuFreqMhz { get; private set; }
    public string GpuName { get; private set; } = "—";

    public SocViewModel()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            var m = HardwareMonitor.Shared;
            CpuPercent = m.CpuUsagePercent; CpuTempC = m.CpuTempC; CpuName = m.CpuName; CoreUsages = m.CpuCoreUsage;
            GpuPercent = m.GpuUsagePercent; GpuTempC = m.GpuTempC; GpuFreqMhz = m.GpuFreqMhz; GpuName = m.GpuName;
            OnPropertyChanged(nameof(CpuPercent), nameof(CpuTempC), nameof(CpuName), nameof(CoreUsages),
                nameof(GpuPercent), nameof(GpuTempC), nameof(GpuFreqMhz), nameof(GpuName));
        }, null, 0, 1000);
    }
}

public class StorageViewModel : ObservableObject
{
    private readonly System.Threading.Timer _timer;
    public long RamTotalGb { get; private set; }
    public long RamUsedGb { get; private set; }
    public float RamPercent { get; private set; }
    public List<DiskDriveInfo> Drives { get; private set; } = new(0, 0);

    public StorageViewModel()
    {
        _timer = new System.Threading.Timer(_ =>
        {
            var m = HardwareMonitor.Shared;
            RamTotalGb = m.RamTotalBytes / 1024 / 1024 / 1024;
            RamUsedGb = m.RamUsedBytes / 1024 / 1024 / 1024;
            RamPercent = m.RamTotalBytes > 0 ? (float)m.RamUsedBytes / m.RamTotalBytes * 100f : 0;

            var drives = new List<DiskDriveInfo>();
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady && drive.TotalSize > 0)
                        drives.Add(new DiskDriveInfo(
                            drive.Name, drive.VolumeLabel,
                            drive.TotalSize / 1024 / 1024 / 1024,
                            (drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024 / 1024));
                } catch { }
            }
            Drives = drives;
            OnPropertyChanged(nameof(RamTotalGb), nameof(RamUsedGb), nameof(RamPercent), nameof(Drives));
        }, null, 0, 3000);
    }
}

public record DiskDriveInfo(string Letter, string Label, long TotalGb, long UsedGb);

public class ScreenViewModel : ObservableObject
{
    public ResolutionInfo Resolution { get; private set; } = new(0, 0);
    public float RefreshRate { get; private set; }
    public int Monitors { get; private set; } = 1;

    public ScreenViewModel()
    {
        Refresh();
    }

    public void Refresh()
    {
        try
        {
            Resolution = new ResolutionInfo(System.Windows.Forms.Screen.PrimaryScreen?.Bounds.Width ?? 0,
                System.Windows.Forms.Screen.PrimaryScreen?.Bounds.Height ?? 0);
            Monitors = System.Windows.Forms.Screen.AllScreens.Length;
            // Refresh rate via GDI
            RefreshRate = QueryRefreshRate();
            OnPropertyChanged(nameof(Resolution), nameof(RefreshRate), nameof(Monitors));
        }
        catch { }
    }

    private static float QueryRefreshRate()
    {
        try
        {
            using var ddc = System.Windows.Forms.Screen.PrimaryScreen;
            // .NET 直接读 PrimaryScreen.DeviceName，通过 user32 GetDeviceCaps
            var name = ddc?.DeviceName ?? "\\\\.\\DISPLAY1";
            var hdc = GetDC(name);
            if (hdc == IntPtr.Zero) return 60;
            try
            {
                var value = GetDeviceCaps(hdc, 116); // VREFRESH
                return value > 0 ? value : 60;
            }
            finally { ReleaseDC(IntPtr.Zero, hdc); }
        }
        catch { return 60; }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetDC(string deviceName);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int index);
}

public record ResolutionInfo(int Width, int Height) { public override string ToString() => $"{Width} × {Height}"; }

public class SystemViewModel : ObservableObject
{
    public string OsCaption { get; private set; } = Environment.OSVersion.VersionString;
    public string MachineName { get; private set; } = Environment.MachineName;
    public string UserName { get; private set; } = Environment.UserName;
    public string Framework { get; private set; } = Environment.Version.ToString();
    public int ProcessorCount { get; private set; } = Environment.ProcessorCount;
    public bool Is64Bit { get; private set; } = Environment.Is64BitOperatingSystem;
    public string ArchitectureText => Is64Bit ? "64 位" : "32 位";
}
