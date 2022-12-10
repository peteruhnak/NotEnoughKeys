using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace NotEnoughKeys.Modules;

internal static class ProcessUtils
{
    public static ProcessInfo? ProcessInfoForWindow(HWND windowHandle)
    {
        if (windowHandle.IsNull) return null;

        uint processId = 0;
        unsafe
        {
#pragma warning disable CA1806
            PInvoke.GetWindowThreadProcessId(WindowUtils.GetForegroundWindow(), &processId);
#pragma warning restore CA1806
        }

        if (processId <= 0) return null;
        try
        {
            using var process = Process.GetProcessById((int)processId);
            return new ProcessInfo
            {
                Id = process.Id,
                ProcessName = process.ProcessName,
                Title = process.MainWindowTitle,
                FileName = process.MainModule?.FileName,
                ModuleName = process.MainModule?.ModuleName
            };
        }
        catch (Exception e)
        {
            GlobalLog.Error("Failed to obtain foreground process information", e);
            return null;
        }
    }
}

internal class ProcessInfo
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string ProcessName { get; init; } = null!;
    public string? FileName { get; init; }
    public string? ModuleName { get; init; }
}