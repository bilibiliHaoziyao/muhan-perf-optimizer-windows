using Timer = System.Threading.Timer;
using System;
using System.Threading;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 后台自动内存清理调度。
/// </summary>
public static class OptimizeService
{
    private static Timer? _timer;
    private static readonly object _lock = new();
    public static bool IsRunning { get; private set; }

    private const int CheckIntervalMs = 3 * 60 * 1000;

    public static void Start()
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
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            IsRunning = false;
        }
    }

    private static void Tick()
    {
        try { MemoryOptimizer.OptimizeAll(); }
        catch { }
    }
}
