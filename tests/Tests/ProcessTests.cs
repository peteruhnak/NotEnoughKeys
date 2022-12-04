using System.Diagnostics;

namespace Tests;

public class ProcessTests
{
    public static void Launch()
    {
        var info = new ProcessStartInfo();
        info.UseShellExecute = true;
        info.FileName = "wt";
        using var process = Process.Start(info);
        // using var process = Process.Start("cmd.exe", new[] { "wt" });
    }
}