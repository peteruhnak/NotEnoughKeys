using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys;

public class HookHandler
{
    private static bool _isSending;
    private static KeyState _capsLockState = KeyState.Up;
    private readonly Dictionary<VIRTUAL_KEY, Binding> _capsBindings = new();

    public HookHandler(Config config)
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

    internal bool HandleKey(WPARAM wParam, KBDLLHOOKSTRUCT lParam)
    {
        var key = (VIRTUAL_KEY)lParam.vkCode;
        KeyState keyState = wParam.Value is PInvoke.WM_KEYDOWN ? KeyState.Down : KeyState.Up;

        // GlobalLog.Debug(
        //     $"{keyState} {key} {lParam.scanCode} {lParam.scanCode:X} {lParam.dwExtraInfo} extended?{(lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_EXTENDED) != 0}");

        if (wParam.Value is not (PInvoke.WM_KEYUP or PInvoke.WM_KEYDOWN)) return false;
        if ((lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_INJECTED) != 0 ||
            (lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_LOWER_IL_INJECTED) != 0) return false;
        if (_isSending) return false;


        if (key == VIRTUAL_KEY.VK_CAPITAL)
        {
            _capsLockState = keyState;
            return true;
        }


        if (_capsLockState == KeyState.Down)
        {
            // action is performed on KeyDown, so ignore KeyUp, maybe we should consume it instead?
            if (keyState == KeyState.Up) return false;

            if (key == VIRTUAL_KEY.VK_I)
            {
                DumpFocusWindowInfo();
                return true;
            }

            if (_capsBindings.TryGetValue(key, out Binding? binding))
            {
                _isSending = true;
                bool result = ActionDispatch.TryExecuteAction(binding);
                _isSending = false;
                return result;
            }

            return false;
        }

        return false;
    }

    private void DumpFocusWindowInfo()
    {
        ProcessInfo? process = ActionDispatch.ForegroundProcessInfo();
        if (process == null)
        {
            GlobalLog.Warn("Unable to determine running process");
            return;
        }

        GlobalLog.Info($"Id: {process.Id}");
        GlobalLog.Info($"Name: {process.ProcessName}");
        GlobalLog.Info($"Title: {process.Title}");
        GlobalLog.Info($"FileName: {process.FileName}");
        GlobalLog.Info($"ModuleName: {process.ModuleName}");
    }
}

internal enum KeyState
{
    Up,
    Down
}