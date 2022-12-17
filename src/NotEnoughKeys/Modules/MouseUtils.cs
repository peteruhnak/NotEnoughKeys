using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Windows.Win32;

namespace NotEnoughKeys.Modules;

internal static class MouseUtils
{
    public static Point GetMousePos()
    {
        PInvoke.GetCursorPos(out var point);
        return new Point(point.X, point.Y);
    }

    public static bool IsMouseDown(MouseKey virtualKey)
    {
        var state = PInvoke.GetAsyncKeyState((int)virtualKey);
        Debug.WriteLine(state);
        return (state & 0x8000) != 0;
    }
    
}

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
[SuppressMessage("ReSharper", "CommentTypo")]
internal enum MouseKey
{
    // Lbutton = 1,
    Left = 1,
    // Rbutton = 2,
    Right = 2,
    // Mbutton = 4,
    Middle = 4,
    // Xbutton1 = 5,
    Back = 5,
    // Xbutton2 = 6,
    Forward = 6,
}