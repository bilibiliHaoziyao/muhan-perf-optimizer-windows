using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 硬件数据统一采集器（单例）。
/// 封装 LibreHardwareMonitor，后台 1s 采样一次，所有 UI 直接读缓存值。
/// 对应 Android 端的 CpuReader / GpuReader / ThermalReader / BatteryReader。
/// </summary>
public sealed class HardwareMonitor : IDisposable
{
    public static HardwareMonitor Shared { get; } = new();

    private Computer? _computer;
    private Timer? _timer;
    private readonly object _lock = new();

    // ============ 缓存值（线程安全）============

    /// <summary>CPU 总体利用率 0-100</summary>
    public float CpuUsagePercent { get; private set; }

    /// <summary>每核 CPU 利用率</summary>
    public float[] CpuCoreUsage { get; private set; } = Array.Empty<float>();

    /// <summary>CPU 总体温度 °C（取最热核心）</summary>
    public float CpuTempC { get; private set; }

    /// <summary>GPU 利用率 0-100</summary>
    public float GpuUsagePercent { get; private set; }

    /// <summary>GPU 温度 °C</summary>
    public float GpuTempC { get; private set; }

    /// <summary>GPU 频率 MHz</summary>
    public float GpuFreqMhz { get; private set; }

    /// <summary>物理内存总字节</summary>
    public long RamTotalBytes { get; private set; }

    /// <summary>物理内存已用字节</summary>
    public long RamUsedBytes { get; private set; }

    /// <summary>主板温度 °C（多个传感器取平均）</summary>
    public float? MotherboardTempC { get; private set; }

    /// <summary>CPU 型号</summary>
    public string CpuName { get; private set; } = "未知";

    /// <summary>GPU 型号</summary>
    public string GpuName { get; private set; } = "未知";

    /// <summary>是否笔记本（有电池）</summary>
    public bool HasBattery { get; private set; }

    /// <summary>电池剩余 %（仅笔记本）</summary>
    public float BatteryPercent { get; private set; } = -1;

    /// <summary>电池充电功率 W（仅笔记本）</summary>
    public float BatteryPowerWatts { get; private set; }

    public bool IsRunning { get; private set; }

public float RamPercent => RamTotalBytes > 0 ? (float)RamUsedBytes * 100 / RamTotalBytes : 0;
    public float RamUsedGb => RamUsedBytes / 1024f / 1024 / 1024;
    public float RamTotalGb => RamTotalBytes / 1024f / 1024 / 1024;
    public float GpuMemoryUsedMb { get; private set; }
    public float GpuMemoryTotalMb { get; private set; }
    public void EnsureStarted() { if (!IsRunning) Start(); }

    public void Start()
    {
        if (IsRunning) return;
        lock (_lock)
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsBatteryEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = false
            };
            _computer.Open();
            _timer = new Timer(_ => Sample(), null, 500, 1000);
            IsRunning = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            IsRunning = false;
            _timer?.Dispose();
            _timer = null;
            _computer?.Close();
            _computer = null;
        }
    }

    private void Sample()
    {
        try
        {
            var comp = _computer;
            if (comp == null) return;
            // comp.Update();

            foreach (var hw in comp.Hardware)
            {
                // hw.Update();
                switch (hw.HardwareType)
                {
                    case HardwareType.Cpu:
                        ProcessCpu(hw); break;
                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        ProcessGpu(hw); break;
                    case HardwareType.Memory:
                        ProcessMemory(hw); break;
                    case HardwareType.Motherboard:
                        ProcessMotherboard(hw); break;
                    case HardwareType.Battery:
                        ProcessBattery(hw); break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HardwareMonitor.Sample 异常: {ex.Message}");
        }
    }

    private void ProcessCpu(IHardware hw)
    {
        if (string.IsNullOrEmpty(CpuName)) CpuName = hw.Name;

        var cpuUsages = new List<float>();
        float packageTemp = float.NaN;
        float maxCoreTemp = float.NaN;

        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value == null) continue;
            // 总体 CPU 利用率（传感器名称含 Total）
            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total"))
                CpuUsagePercent = sensor.Value.Value;
            // 每核利用率
            else if (sensor.SensorType == SensorType.Load && sensor.Name.StartsWith("Core #"))
                cpuUsages.Add(sensor.Value.Value);
            // 封装温度
            else if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Package"))
                packageTemp = sensor.Value.Value;
            // 核心温度最高值
            else if (sensor.SensorType == SensorType.Temperature && sensor.Name.StartsWith("Core"))
                maxCoreTemp = float.IsNaN(maxCoreTemp) ? sensor.Value.Value : Math.Max(maxCoreTemp, sensor.Value.Value);
        }

        // 优先 Package 温度，否则核心最高
        if (!float.IsNaN(packageTemp)) CpuTempC = packageTemp;
        else if (!float.IsNaN(maxCoreTemp)) CpuTempC = maxCoreTemp;

        CpuCoreUsage = cpuUsages.ToArray();
    }

    private void ProcessGpu(IHardware hw)
    {
        if (string.IsNullOrEmpty(GpuName)) GpuName = hw.Name;

        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value == null) continue;
            switch (sensor.SensorType)
            {
                case SensorType.Load:
                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("3D") || sensor.Name.Contains("GPU"))
                        GpuUsagePercent = sensor.Value.Value;
                    break;
                case SensorType.Temperature:
                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("GPU"))
                        GpuTempC = sensor.Value.Value;
                    break;
                case SensorType.Clock:
                    if (sensor.Name.Contains("Core") || sensor.Name.Contains("GPU"))
                        GpuFreqMhz = sensor.Value.Value;
                    break;
            }
        }
    }

    private void ProcessMemory(IHardware hw)
    {
        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value == null) continue;
            switch (sensor.SensorType)
            {
                case SensorType.Data:
                    if (sensor.Name.Contains("Used")) RamUsedBytes = (long)(sensor.Value.Value * 1024 * 1024);
                    else if (sensor.Name.Contains("Available"))
                    {
                        // Available 在 LibreHW 是 Used 的互补
                    }
                    break;
                case SensorType.SmallData:
                    // Total (GB)
                    if (sensor.Name.Contains("Available"))
                        RamTotalBytes = (long)(sensor.Value.Value * 1024 * 1024 * 1024);
                    break;
            }
        }
    }

    private void ProcessMotherboard(IHardware hw)
    {
        var temps = hw.Sensors
            .Where(s => s.SensorType == SensorType.Temperature && s.Value != null && s.Name.Contains("Temperature"))
            .Select(s => s.Value!.Value)
            .ToList();
        if (temps.Count > 0) MotherboardTempC = temps.Average();
    }

    private void ProcessBattery(IHardware hw)
    {
        HasBattery = true;
        foreach (var sensor in hw.Sensors)
        {
            if (sensor.Value == null) continue;
            switch (sensor.SensorType)
            {
                case SensorType.Level:
                    BatteryPercent = sensor.Value.Value; break;
                case SensorType.Power:
                    BatteryPowerWatts = sensor.Value.Value; break;
            }
        }
    }

    public void Dispose() => Stop();
}
