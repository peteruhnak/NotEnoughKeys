using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys.Modules;

internal static class WindowUtils
{
    public static HWND GetForegroundWindow()
    {
        return PInvoke.GetForegroundWindow();
    }

    public static Rectangle GetWindowRect(HWND hwnd)
    {
        PInvoke.GetWindowRect(hwnd, out var rect);
        return new Rectangle { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
    }

    public static void SetWindowRect(HWND hwnd, Rectangle rect)
    {
        PInvoke.SetWindowPos(hwnd, HWND.HWND_NOTOPMOST,
            rect.X, rect.Y, rect.Width, rect.Height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
    }

    public static Rectangle GetClientRect(HWND hwnd)
    {
        PInvoke.GetClientRect(hwnd, out var rect);
        return new Rectangle { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
    }

    public static void SetWindowPos(HWND hwnd, Point p)
    {
        SetWindowPos(hwnd, p.X, p.Y);
    }

    public static void SetWindowPos(HWND hwnd, int x, int y)
    {
        PInvoke.SetWindowPos(hwnd, HWND.HWND_NOTOPMOST,
            x, y, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Stuff()
    {
        var process = Process.GetProcessesByName("notepad")[0];
        var hwnd = new HWND(process.MainWindowHandle);
        // var hwnd = GetForegroundWindow();
        var wr = GetWindowRect(hwnd);
        var cr = GetClientRect(hwnd);

        Console.WriteLine($"{wr.X}x{wr.Y}");
        Console.WriteLine($"{cr.X}x{cr.Y}");

        Console.WriteLine(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXBORDER));
        Console.WriteLine(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYBORDER));
        Console.WriteLine(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYCAPTION));

        WINDOWINFO w = default;
        unsafe
        {
            w.cbSize = (uint)sizeof(WINDOWINFO);
            if (PInvoke.GetWindowInfo(hwnd, ref w))
            {
                Console.WriteLine(w.cxWindowBorders);
                Console.WriteLine(w.cyWindowBorders);
                Console.WriteLine(w.dwStyle);
                Console.WriteLine(w.dwExStyle);
                Console.WriteLine($"{w.rcClient.X}x{w.rcClient.Y}");
                Console.WriteLine($"{w.rcWindow.X}x{w.rcWindow.Y}");
            }
        }

        // process.MainWindowHandle;
        //SetWindowPos(new HWND(process.MainWindowHandle), -1200, 0);
        // Console.WriteLine(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN));
        // Console.WriteLine(PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN));
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal class MonitorInfoLoader
{
    private List<Rectangle>? _rectangles;

    public static void Test()
    {
        foreach (var rectangle in new MonitorInfoLoader().GetMonitorInfo())
        {
            Console.WriteLine(rectangle);
        }
    }

    public List<Rectangle> GetMonitorInfo()
    {
        if (_rectangles == null)
            LoadMonitorInfo();
        return _rectangles!;
    }

    public void LoadMonitorInfo()
    {
        _rectangles = new List<Rectangle>();
        unsafe
        {
            if (!PInvoke.EnumDisplayMonitors(null, null, Callback, IntPtr.Zero))
            {
                GlobalLog.Warn("Failed to EnumDisplayMonitors()");
            }
        }
    }

    private unsafe BOOL Callback(HMONITOR hMon, HDC hdcMon, RECT* rect, LPARAM data)
    {
        MONITORINFO info = default;
        info.cbSize = (uint)sizeof(MONITORINFO);
        if (PInvoke.GetMonitorInfo(hMon, ref info))
        {
            _rectangles!.Add(ToRectangle(info.rcMonitor));
        }
        else
        {
            Console.WriteLine("Failed to GetMonitorInfo()");
        }

        return true;
    }

    private static Rectangle ToRectangle(RECT rect) =>
        new() { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
}