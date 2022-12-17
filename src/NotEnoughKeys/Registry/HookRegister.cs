using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys.Registry;

internal class HookRegister : IDisposable
{
    private HOOKPROC _kbdProc = null!;
    private SafeHandle? _kbdHandle;
    private IKeyboardLowLevelHookHandler? _hookHandler;
    private bool _isDisposed;

    public void Register(IKeyboardLowLevelHookHandler hookHandler)
    {
        _hookHandler = hookHandler;
        _kbdProc = KeyboardLowLevelProc;
        GlobalLog.Info("Registering keyboard low level hook");
        _kbdHandle = SetHook(_kbdProc, WINDOWS_HOOK_ID.WH_KEYBOARD_LL);
    }
    
    public void Unregister()
    {
        GlobalLog.Info("Unregistering keyboard low level hook");
        if (_kbdHandle != null)
            RemoveHook(_kbdHandle);
    }

    private LRESULT KeyboardLowLevelProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0
            && _hookHandler != null
            && _hookHandler.HandleKey(wParam, Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)))
            return new LRESULT(1);

        return PInvoke.CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    private SafeHandle SetHook(HOOKPROC hookProc, WINDOWS_HOOK_ID windowsHookId)
    {
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        return PInvoke.SetWindowsHookEx(windowsHookId, hookProc,
            PInvoke.GetModuleHandle(module?.ModuleName), 0);
    }

    private void RemoveHook(SafeHandle unhookWindowsHookExSafeHandle)
    {
        PInvoke.UnhookWindowsHookEx(new HHOOK(unhookWindowsHookExSafeHandle.DangerousGetHandle()));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool isDisposing)
    {
        if (_isDisposed) return;
        if (isDisposing)
        {
            Unregister();
            _kbdHandle?.Dispose();
        }

        _isDisposed = true;
    }
}