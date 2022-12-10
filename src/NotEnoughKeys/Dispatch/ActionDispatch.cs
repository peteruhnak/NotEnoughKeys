using System.Diagnostics;
using NotEnoughKeys.Handlers;
using NotEnoughKeys.Modules;

namespace NotEnoughKeys.Dispatch;

public static class ActionDispatch
{
    public static bool TryExecuteAction(Binding action)
    {
        if (!WhenMatches(action.When))
            return false;
        if (action.Send is { } sendKeys)
        {
            KeyUtils.SendKeyPresses(sendKeys);
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
                using var process = Process.Start(startInfo);
                if (process != null)
                    GlobalLog.Info($"Launched {process.Id} {process.ProcessName}");
                else
                    GlobalLog.Error($"Failed to start process {run}");
            }
            catch (Exception e)
            {
                GlobalLog.Error($"Failed to start process {run}", e);
            }

            return true;
        }

        if (action.Special is { } special)
        {
            SpecialHandler.HandleSpecial(special);
        }

        return true;
    }

    public static bool WhenMatches(WhenCondition? when)
    {
        if (when == null) return true;
        var info = ProcessUtils.ProcessInfoForWindow(WindowUtils.GetForegroundWindow());
        if (info == null) return false;

        return (when.Exe == null || (info.FileName ?? "").Equals(when.Exe, StringComparison.InvariantCultureIgnoreCase))
               && (when.Title == null || info.Title.Equals(when.Title, StringComparison.InvariantCultureIgnoreCase));
    }
}