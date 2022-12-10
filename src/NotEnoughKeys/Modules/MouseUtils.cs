using Windows.Win32;

namespace NotEnoughKeys.Modules;

public static class MouseUtils
{
    public static Point GetMousePos()
    {
        PInvoke.GetCursorPos(out var point);
        return new Point(point.X, point.Y);
    }

}