using System.Diagnostics;
using Windows.Win32;

namespace NotEnoughKeys;

public static class ActionDispatch
{
    public static bool TryExecuteAction(Binding action)
    {
        if (!WhenMatches(action.When))
            return false;
        if (action.Send is { } sendKeys)
        {
            InputSender.SendKeyPresses(sendKeys);
            return true;
        }

        if (action.Run is { } run)
        {
            GlobalLog.Info($"Starting process {run}");
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = run;
            if (!(run.Contains('/') || run.Contains('\\')))
                startInfo.UseShellExecute = true;

            try
            {
                using Process? process = Process.Start(startInfo);
                if (process != null)
                    GlobalLog.Info($"Launched {process.Id} {process.ProcessName}");
                else
                    GlobalLog.Error($"Failed to start process {run}");
            }
            catch (Exception e)
            {
                GlobalLog.Error($"Failed to start process {run}", e);
            }
        }

        return true;
    }

    public static bool WhenMatches(WhenCondition? when)
    {
        if (when == null) return true;
        ProcessInfo? info = ForegroundProcessInfo();
        if (info == null) return false;

        return (when.Exe == null || (info.FileName ?? "").Equals(when.Exe, StringComparison.InvariantCultureIgnoreCase))
               && (when.Title == null || info.Title.Equals(when.Title, StringComparison.InvariantCultureIgnoreCase));
    }

    public static ProcessInfo? ForegroundProcessInfo()
    {
        uint processId = 0;
        unsafe
        {
#pragma warning disable CA1806
            PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow(), &processId);
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

public class ProcessInfo
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
    public string ProcessName { get; init; } = null!;
    public string? FileName { get; init; }
    public string? ModuleName { get; init; }
}