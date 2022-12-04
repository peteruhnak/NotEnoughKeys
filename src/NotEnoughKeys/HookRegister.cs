using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys;

public class HookRegister : IDisposable
{
    private HOOKPROC _proc = null!;
    private SafeHandle? _hookHandle;
    private HookHandler? _hookHandler;
    private bool _isDisposed;

    private LRESULT KeyboardLowLevelProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644984(v=vs.85)#parameters
        //  If code is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.
        if (nCode >= 0
            && _hookHandler != null
            && _hookHandler.HandleKey(wParam, Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)))
        {
            // PInvoke.PostMessage(HHOOK.Null, PInvoke.WM_UPInvoke.WM_, wParam, lParam)l
            return new LRESULT(1);
        }


        return PInvoke.CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    public void Register(HookHandler hookHandler)
    {
        _hookHandler = hookHandler;
        _proc = KeyboardLowLevelProc;
        GlobalLog.Info("Registering keyboard low level hook");
        _hookHandle = SetHook(_proc);
    }

    private SafeHandle SetHook(HOOKPROC hookProc)
    {
        using var process = Process.GetCurrentProcess();
        using ProcessModule? module = process.MainModule;
        return PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, hookProc,
            PInvoke.GetModuleHandle(module?.ModuleName), 0);
    }

    private void RemoveHook(SafeHandle unhookWindowsHookExSafeHandle)
    {
        PInvoke.UnhookWindowsHookEx(new HHOOK(unhookWindowsHookExSafeHandle.DangerousGetHandle()));
    }

    public void Unregister()
    {
        GlobalLog.Info("Unregistering keyboard low level hook");
        if (_hookHandle == null) return;
        RemoveHook(_hookHandle);
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
            _hookHandle?.Dispose();
        }

        _isDisposed = true;
    }
}