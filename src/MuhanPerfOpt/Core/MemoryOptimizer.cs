using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace MuhanPerfOpt.Core;

/// <summary>
/// 内存清理：强制所有用户进程释放工作集 + 刷新系统文件缓存。
/// 对应 Android 端 MemoryCleaner.cleanBackgroundProcesses。
/// Windows 没有"杀后台"的 API，这里采用 SetProcessWorkingSetSize 让进程主动归还物理内存。
/// </summary>
public static class MemoryOptimizer
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimum, IntPtr dwMaximum);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetSystemFileCacheSize(IntPtr FileCacheSize, IntPtr MinFreeCache, SetSystemFileCacheSizeFlags Flags);

    [Flags]
    private enum SetSystemFileCacheSizeFlags { NOFLUSH = 0x00000001, NOGROW = 0x00000002 }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    /// <summary>
    /// 对所有非系统、非当前进程调用 EmptyWorkingSet，让它们把驻留工作集分页到磁盘。
    /// 返回成功清理的进程数。需要管理员权限。
    /// </summary>
    public static CleanResult CleanProcesses(ISet<string>? whitelist = null)
    {
        var result = new CleanResult();
        var selfId = Process.GetCurrentProcess().Id;
        var selfName = Process.GetCurrentProcess().ProcessName;
        whitelist ??= new HashSet<string>();

        IEnumerable<Process> processes;
        try { processes = Process.GetProcesses(); }
        catch { return result; }

        foreach (var p in processes)
        {
            result.Attempted++;
            try
            {
                if (p.Id == selfId) { result.Skipped++; continue; }
                var name = p.ProcessName;
                if (whitelist.Contains(name)) { result.Skipped++; p.Dispose(); continue; }

                var handle = p.Handle;
                if (SetProcessWorkingSetSize(handle, new IntPtr(-1), new IntPtr(-1)))
                    result.Succeeded++;
                else
                    result.Failed++;
            }
            catch
            {
                // 无权限/已退出/系统进程
                result.Skipped++;
            }
            finally
            {
                try { p.Dispose(); } catch { }
            }
        }

        // 刷新当前进程自身
        SetProcessWorkingSetSize(GetCurrentProcess(), new IntPtr(-1), new IntPtr(-1));
        result.Succeeded++;

        return result;
    }

    /// <summary>刷新系统文件缓存（让内核放弃脏页和缓存页）</summary>
    public static bool PurgeSystemFileCache()
    {
        try
        {
            // 将系统文件缓存设为 0 字节（NOFLUSH=跳过写盘，更快）
            var ok = SetSystemFileCacheSize(
                new IntPtr(0), new IntPtr(0), SetSystemFileCacheSizeFlags.NOFLUSH | SetSystemFileCacheSizeFlags.NOGROW);
            return ok;
        }
        catch { return false; }
    }

    public struct CleanResult
    {
        public int Attempted;
        public int Succeeded;
        public int Skipped;
        public int Failed;
    }
}
