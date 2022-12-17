using NotEnoughKeys.Handlers;
using NotEnoughKeys.Registry;

namespace NotEnoughKeys;

public class KeyboardManager : IDisposable
{
    private HookRegister _hookRegister = null!;
    private KeyboardLowLevelHookHandler _keyboardLowLevelHookHandler = null!;
    private HotkeyRegister _hotkeyRegister = null!;
    private HotkeyHandler _hotkeyHandler = null!;
    private bool _isDisposed;

    public void Initialize(Config config)
    {
        _keyboardLowLevelHookHandler = new KeyboardLowLevelHookHandler(config);
        _hookRegister = new HookRegister();

        _hotkeyHandler = new HotkeyHandler();
        _hotkeyRegister = new HotkeyRegister(config);

        _hookRegister.Register(_keyboardLowLevelHookHandler);
        _hotkeyRegister.Register(_hotkeyHandler);
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
            _hookRegister.Dispose();
            _hotkeyRegister.Dispose();
        }

        _isDisposed = true;
    }
}