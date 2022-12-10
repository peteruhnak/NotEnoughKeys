using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using NotEnoughKeys.Handlers;

namespace NotEnoughKeys.Registry;

public class HookRegister : IDisposable
{
    private HOOKPROC _kbdProc = null!;
    private HOOKPROC _mouseProc = null!;
    private SafeHandle? _kbdHandle;
    private SafeHandle? _mouseHandle;
    private KeyboardLowLevelHookHandler? _keyboardHookHandler;
    private MouseLowLevelHookHandler? _mouseHookHandler;
    private bool _isDisposed;

    public void Register(KeyboardLowLevelHookHandler keyboardLowLevelHookHandler)
    {
        _keyboardHookHandler = keyboardLowLevelHookHandler;
        _kbdProc = KeyboardLowLevelProc;
        GlobalLog.Info("Registering keyboard low level hook");
        _kbdHandle = SetHook(_kbdProc, WINDOWS_HOOK_ID.WH_KEYBOARD_LL);
    }

    public void Register(MouseLowLevelHookHandler mouseLowLevelHookHandler)
    {
        _mouseHookHandler = mouseLowLevelHookHandler;
        _mouseProc = MouseLowLevelProc;
        GlobalLog.Info("Registering mouse low level hook");
        _mouseHandle = SetHook(_mouseProc, WINDOWS_HOOK_ID.WH_MOUSE_LL);
    }
    
    public void Unregister()
    {
        GlobalLog.Info("Unregistering keyboard low level hook");
        if (_kbdHandle != null)
            RemoveHook(_kbdHandle);
        if (_mouseHandle != null)
            RemoveHook(_mouseHandle);
    }

    private LRESULT KeyboardLowLevelProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        // https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644984(v=vs.85)#parameters
        //  If code is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.
        if (nCode >= 0
            && _keyboardHookHandler != null
            && _keyboardHookHandler.HandleKey(wParam, Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam)))
            return new LRESULT(1);

        return PInvoke.CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    private LRESULT MouseLowLevelProc(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0
            && _mouseHookHandler != null
            && _mouseHookHandler.Handle(wParam, Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam)))
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