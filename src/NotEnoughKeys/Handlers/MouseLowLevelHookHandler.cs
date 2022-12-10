using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
// ReSharper disable All

namespace NotEnoughKeys.Handlers;

public class MouseLowLevelHookHandler
{
    // private static bool _isSending;
    // private static KeyState _capsLockState = KeyState.Up;
    private readonly Dictionary<VIRTUAL_KEY, Binding> _capsBindings = new();

    public MouseLowLevelHookHandler(Config config)
    {
        ReloadConfig(config);
    }

    private void ReloadConfig(Config config)
    {
        foreach (Binding binding in config.Hooks)
        {
            if (binding.Keys.Contains(VirtualKey.CapsLock))
            {
                var keys = binding.Keys.Except(new[] { VirtualKey.CapsLock }).ToList();
                if (keys.Count == 1)
                {
                    _capsBindings.Add((VIRTUAL_KEY)keys[0], binding);
                }
            }
        }
    }

    internal bool Handle(WPARAM wParam, MSLLHOOKSTRUCT lParam)
    {
        // if (wParam.Value is not (PInvoke.WM_KEYUP or PInvoke.WM_KEYDOWN)) return false;
        var flags = (MSLLHOOKSTRUCT_FLAGS)lParam.flags;
        if ((flags & MSLLHOOKSTRUCT_FLAGS.LLMHF_INJECTED) != 0 ||
            (flags & MSLLHOOKSTRUCT_FLAGS.LLMHF_LOWER_IL_INJECTED) != 0) return false;

        var btn = (uint)wParam.Value;
        GlobalLog.Debug($"{btn} {lParam.pt.X}x{lParam.pt.Y} {lParam.mouseData:X}");
        // var key = (VIRTUAL_KEY)lParam.vkCode;
        // KeyState keyState = wParam.Value is PInvoke.WM_KEYDOWN ? KeyState.Down : KeyState.Up;

        // // GlobalLog.Debug(
        // //     $"{keyState} {key} {lParam.scanCode} {lParam.scanCode:X} {lParam.dwExtraInfo} extended?{(lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_EXTENDED) != 0}");
        //
        // if (wParam.Value is not (PInvoke.WM_KEYUP or PInvoke.WM_KEYDOWN)) return false;
        // if ((lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_INJECTED) != 0 ||
        //     (lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_LOWER_IL_INJECTED) != 0) return false;
        // if (_isSending) return false;
        //
        //
        // if (key == VIRTUAL_KEY.VK_CAPITAL)
        // {
        //     _capsLockState = keyState;
        //     return true;
        // }
        //
        //
        // if (_capsLockState == KeyState.Down)
        // {
        //     // action is performed on KeyDown, so ignore KeyUp, maybe we should consume it instead?
        //     if (keyState == KeyState.Up) return false;
        //
        //     if (key == VIRTUAL_KEY.VK_I)
        //     {
        //         DumpFocusWindowInfo();
        //         return true;
        //     }
        //
        //     if (_capsBindings.TryGetValue(key, out Binding? binding))
        //     {
        //         _isSending = true;
        //         bool result = ActionDispatch.TryExecuteAction(binding);
        //         _isSending = false;
        //         return result;
        //     }
        //
        //     return false;
        // }
        return false;
    }
}

[Flags]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum MSLLHOOKSTRUCT_FLAGS : uint
{
    LLMHF_INJECTED = 0x00000001,
    LLMHF_LOWER_IL_INJECTED = 0x00000002,
}