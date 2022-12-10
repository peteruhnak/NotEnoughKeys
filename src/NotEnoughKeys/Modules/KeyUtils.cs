using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace NotEnoughKeys.Modules;

using VK = VirtualKey;

public static class KeyUtils
{
    internal static void SendKeyPresses(IEnumerable<VirtualKey> virtualKey)
    {
        var inputs = new List<INPUT>();
        foreach (var k in virtualKey)
        {
            inputs.Add(KeyDown((VIRTUAL_KEY)k));
            inputs.Add(KeyUp((VIRTUAL_KEY)k));
        }

        SendInput(inputs.ToArray());
    }

    internal static void SendInput(INPUT[] inputs)
    {
        var span = new Span<INPUT>(inputs);
        if (PInvoke.SendInput(span, Marshal.SizeOf(typeof(INPUT))) != span.Length)
        {
            var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            GlobalLog.Error($"SendInput() failed: {errorMessage}");
        }
    }

    internal static INPUT KeyDown(VIRTUAL_KEY virtualKey)
    {
        return Key(new KEYBDINPUT
        {
            wVk = virtualKey,
            dwFlags = IsExtended(virtualKey)
        });
    }


    internal static INPUT KeyUp(VIRTUAL_KEY virtualKey)
    {
        return Key(new KEYBDINPUT
        {
            wVk = virtualKey,
            dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP | IsExtended(virtualKey)
        });
    }

    private static KEYBD_EVENT_FLAGS IsExtended(VIRTUAL_KEY virtualKey)
        => KeyMapper.IsExtended((VirtualKey)virtualKey) ? KEYBD_EVENT_FLAGS.KEYEVENTF_EXTENDEDKEY : 0;

    internal static INPUT Key(KEYBDINPUT key)
    {
        var input = new INPUT { type = INPUT_TYPE.INPUT_KEYBOARD };
        input.Anonymous.ki = key;
        return input;
    }
}