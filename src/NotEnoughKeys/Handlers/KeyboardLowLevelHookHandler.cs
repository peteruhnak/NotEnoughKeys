using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using NotEnoughKeys.Dispatch;
using NotEnoughKeys.Modules;
using NotEnoughKeys.Registry;

namespace NotEnoughKeys.Handlers;

internal class KeyboardLowLevelHookHandler : IKeyboardLowLevelHookHandler
{
    private static bool _isSending;
    private static KeyState _capsLockState = KeyState.Up;
    private readonly Dictionary<VIRTUAL_KEY, Binding> _capsBindings = new();
    private readonly Dictionary<VIRTUAL_KEY, SpecialWrapper> _liveBindings = new();

    public KeyboardLowLevelHookHandler(Config config)
    {
        ReloadConfig(config);
    }

    private void ReloadConfig(Config config)
    {
        foreach (var binding in config.Hooks)
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

    public bool HandleKey(WPARAM wParam, KBDLLHOOKSTRUCT lParam)
    {
        var key = (VIRTUAL_KEY)lParam.vkCode;
        var keyState = wParam.Value is PInvoke.WM_KEYDOWN ? KeyState.Down : KeyState.Up;

        // GlobalLog.Debug(
        //     $"{keyState} {key} {lParam.scanCode} {lParam.scanCode:X} {lParam.dwExtraInfo} extended?{(lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_EXTENDED) != 0}");

        if (wParam.Value is not (PInvoke.WM_KEYUP or PInvoke.WM_KEYDOWN)) return false;
        if ((lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_INJECTED) != 0 ||
            (lParam.flags & KBDLLHOOKSTRUCT_FLAGS.LLKHF_LOWER_IL_INJECTED) != 0) return false;
        if (_isSending) return false;

        if (key == VIRTUAL_KEY.VK_CAPITAL)
        {
            _capsLockState = keyState;
            if (keyState == KeyState.Up)
            {
                foreach (var (_, wrapper) in _liveBindings)
                    wrapper.Stop();
                _liveBindings.Clear();
            }

            return true; // always consume CapsLock change
        }

        if (_capsLockState == KeyState.Up) return false; // currently only CapsLock-based shortcuts are permitted for global hook

        if (keyState == KeyState.Up)
        {
            if (_liveBindings.TryGetValue(key, out var wrapper))
            {
                wrapper.Stop();
                _liveBindings.Remove(key);
                return true;
            }

            // regular actions are performed on KeyDown, so ignore KeyUp, maybe we should consume it instead?
            return false;
        }

        if (_liveBindings.ContainsKey(key))
            return true; // ignore repeated events for already active live bindings

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (key)
        {
            case VIRTUAL_KEY.VK_LWIN:
                if (SpecialHandler.HandleSpecial2("MoveWindow") is { } moveWindowWrapper)
                    _liveBindings.Add(key, moveWindowWrapper);
                return true;
            case VIRTUAL_KEY.VK_Q:
                if (SpecialHandler.HandleSpecial2("MoveWindow") is { } wrapper1)
                    _liveBindings.Add(key, wrapper1);
                return true;
            case VIRTUAL_KEY.VK_E:
                if (SpecialHandler.HandleSpecial2("ResizeWindow") is { } wrapper2)
                    _liveBindings.Add(key, wrapper2);
                return true;
            case VIRTUAL_KEY.VK_I:
                DumpFocusWindowInfo();
                return true;
        }

        if (_capsBindings.TryGetValue(key, out var binding))
        {
            _isSending = true;
            var result = ActionDispatch.TryExecuteAction(binding);
            _isSending = false;
            return result;
        }

        return false;
    }

    private void DumpFocusWindowInfo()
    {
        var hwnd = WindowUtils.GetForegroundWindow();
        if (hwnd.IsNull)
        {
            GlobalLog.Warn("Unable to determine foreground window");
            return;
        }

        var process = ProcessUtils.ProcessInfoForWindow(hwnd);
        if (process == null)
        {
            GlobalLog.Warn("Unable to determine running process");
            return;
        }

        var rect = WindowUtils.GetWindowRect(hwnd);

        var msg = "Foreground Window\r\n" +
                  $"Id: {process.Id}\r\n" +
                  $"Name: {process.ProcessName}\r\n" +
                  $"Title: {process.Title}\r\n" +
                  $"FileName: {process.FileName}\r\n" +
                  $"ModuleName: {process.ModuleName}\r\n" +
                  $"Pos: {rect.X}x{rect.Y}\r\n" +
                  $"Size: {rect.Width}x{rect.Height}\r\n";
        GlobalLog.Info(msg);
    }
}

internal enum KeyState
{
    Up,
    Down
}