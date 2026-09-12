using System;
using System.Threading;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 后台自动清理调度。周期采样 RAM 占用，超过阈值时触发一次 CleanProcesses + PurgeSystemFileCache。
/// 对应 Android 端 OptimizeService。
/// </summary>
public static class OptimizeService
{
    private static Timer? _timer;
    private static readonly object _lock = new();
    public static bool IsRunning { get; private set; }

    private const int CheckIntervalMs = 3 * 60 * 1000; // 3 分钟
    private const int MaxCheckIntervalMs = 30 * 60 * 1000;

    public static void StartAutoClean()
    {
        lock (_lock)
        {
            if (IsRunning) return;
            _timer = new Timer(_ => Tick(), null, 30000, CheckIntervalMs);
            IsRunning = true;
        }
    }

    public static void Stop()
    {
        lock (_lock)
        {
            IsRunning = false;
            _timer?.Dispose();
            _timer = null;
        }
    }

    private static void Tick()
    {
        try
        {
            var monitor = HardwareMonitor.Shared;
            if (monitor.RamTotalBytes <= 0) return;

            var usedPct = (double)monitor.RamUsedBytes / monitor.RamTotalBytes * 100;
            var threshold = SettingsService.Current.CleanThresholdPercent;
            if (usedPct >= threshold)
            {
                var result = MemoryOptimizer.CleanProcesses();
                MemoryOptimizer.PurgeSystemFileCache();
                if (result.Succeeded > 0)
                {
                    SettingsService.IncrementCleanStat();
                    ToastService.ShowOptimized(result.Succeeded);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OptimizeService.Tick 异常: {ex.Message}");
        }
    }
}
