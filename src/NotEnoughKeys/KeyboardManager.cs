namespace NotEnoughKeys;

public class KeyboardManager : IDisposable
{
    private HookRegister _hookRegister = null!;
    private HookHandler _hookHandler = null!;
    private HotkeyRegister _hotkeyRegister = null!;
    private HotkeyHandler _hotkeyHandler = null!;
    private bool _isDisposed;

    public void Initialize(Config config)
    {
        _hookHandler = new HookHandler(config);
        _hookRegister = new HookRegister();

        _hotkeyHandler = new HotkeyHandler();
        _hotkeyRegister = new HotkeyRegister(config);

        _hookRegister.Register(_hookHandler);
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