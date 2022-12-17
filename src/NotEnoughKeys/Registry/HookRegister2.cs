using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys.Registry;

internal class HookRegister2 : IDisposable
{
    private HOOKPROC _hookProc = null!;
    private SafeHandle? _safeHandle;
    private IMouseLowLevelHookHandler? _hookHandler;
    private bool _isDisposed;

    public void Register(IMouseLowLevelHookHandler mouseLowLevelHookHandler)
    {
        GlobalLog.Info("Registering mouse low level hook");
        _hookHandler = mouseLowLevelHookHandler;
        _hookProc = MouseLowLevelProc;
        _safeHandle = SetHook(_hookProc, WINDOWS_HOOK_ID.WH_MOUSE_LL);
    }

    public void Unregister()
    {
        if (_safeHandle == null) return;
        GlobalLog.Info("Unregistering mouse low level hook");
        PInvoke.UnhookWindowsHookEx(new HHOOK(_safeHandle.DangerousGetHandle()));
        _safeHandle.Dispose();
        _safeHandle = null;
        _hookHandler = null;
    }

    private LRESULT MouseLowLevelProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0
            && _hookHandler != null
            && _hookHandler.Handle(wParam, Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam)))
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool isDisposing)
    {
        if (_isDisposed) return;
        if (isDisposing)
            Unregister();
        _isDisposed = true;
    }
}