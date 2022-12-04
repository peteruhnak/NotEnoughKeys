using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace NotEnoughKeys;

public class HotkeyRegister : IDisposable
{
    public static event EventHandler<HotkeyEventArgs>? HotKeyPressed;

    private static readonly HashSet<int> HotkeyIds = new();

    private int _idCounter;

    private bool _isDisposed;

    private static readonly MessageWindow Wnd = new();
    private readonly IList<Binding> _hotkeys;
    private HotkeyHandler? _hotkeyHandler;

    public HotkeyRegister(Config config)
    {
        _hotkeys = config.Hotkeys;
    }


    public void Register(HotkeyHandler hotkeyHandler)
    {
        _hotkeyHandler = hotkeyHandler;
        foreach (Binding binding in _hotkeys)
        {
            if (binding.Modifiers != null && binding.Keys.Length == 1)
            {
                RegisterHotkey(binding.Modifiers ?? 0, binding.Keys[0]);
            }
        }

        HotKeyPressed += OnHotKeyPressed;
    }

    private void OnHotKeyPressed(object? sender, HotkeyEventArgs e)
    {
        Binding? binding = _hotkeys.FirstOrDefault(b =>
            b.Keys.Length == 1 && b.Keys[0] == e.Key && e.Modifiers == (b.Modifiers ?? 0));
        if (binding != null)
            _hotkeyHandler?.HandleHotkey(binding);
        else
            GlobalLog.Warn($"Expected a binding for {e.Modifiers} & {e.Key}");
    }

    public void RegisterHotkey(Modifiers modifiers, VirtualKey key)
    {
        int id = Interlocked.Increment(ref _idCounter);
        if (PInvoke.RegisterHotKey((HWND)Wnd.Handle, id, (HOT_KEY_MODIFIERS)modifiers, (uint)key))
        {
            HotkeyIds.Add(id);
        }
        else
        {
            GlobalLog.Error($"Key registration failed {id} {modifiers} {key}");
        }
    }

    private static void UnregisterAllHotkeys()
    {
        GlobalLog.Info("Unregistering all hotkeys");
        foreach (int hotkeyId in HotkeyIds)
        {
            UnregisterHotKey(hotkeyId);
        }

        HotkeyIds.Clear();
    }

    private static void UnregisterHotKey(int id)
    {
        PInvoke.UnregisterHotKey((HWND)Wnd.Handle, id);
    }

    internal static void OnHotKeyPressed(HotkeyEventArgs e)
    {
        HotKeyPressed?.Invoke(null, e);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        if (disposing)
        {
            UnregisterAllHotkeys();
        }

        _isDisposed = true;
    }


    private class MessageWindow : Form
    {
        protected override void WndProc(ref Message m)
        {
            // GlobalLog.Debug(m.ToString());
            switch ((uint)m.Msg)
            {
                case PInvoke.WM_HOTKEY:
                {
                    HotkeyEventArgs e = new(m.LParam);
                    OnHotKeyPressed(e);
                    break;
                }
                case PInvoke.WM_DESTROY:
                    UnregisterAllHotkeys();
                    break;
            }

            base.WndProc(ref m);
        }
    }
}

public class HotkeyEventArgs : EventArgs
{
    public VirtualKey Key { get; }
    public Modifiers Modifiers { get; }

    public HotkeyEventArgs(IntPtr hotKeyParam)
    {
        long dword = hotKeyParam.ToInt64();
        Key = (VirtualKey)(dword >> 16);
        Modifiers = (Modifiers)(dword & 0xFFFF);
    }
}