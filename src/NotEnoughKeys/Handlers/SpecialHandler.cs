using Windows.Win32;
using NotEnoughKeys.Modules;
using NotEnoughKeys.Tiling;

namespace NotEnoughKeys.Handlers;

public static class SpecialHandler
{
    private static MoveWindowHandler? _moveWindowHandler;

    public static void HandleSpecial(string special)
    {
    }

    public static SpecialWrapper? HandleSpecial2(string special)
    {
        var wrapper = special switch
        {
            "MoveWindow" => new SpecialWrapper { OnStart = MoveWindowStart, OnStop = MoveWindowEnd },
            "ResizeWindow" => new SpecialWrapper { OnStart = MoveWindowStart, OnStop = MoveWindowEnd },
            _ => null
        };
        wrapper?.Start();
        return wrapper;
    }

    public static void MoveWindowStart()
    {
        if (_moveWindowHandler == null)
        {
            var hwnd = WindowUtils.GetForegroundWindow();
            if (hwnd.IsNull) return;
            _moveWindowHandler = new MoveWindowHandler(hwnd);
            _moveWindowHandler.Start();
        }
    }

    public static void MoveWindowEnd()
    {
        _moveWindowHandler?.Stop();
        _moveWindowHandler = null;
    }
}

public class SpecialWrapper
{
    public Action OnStart { get; init; } = null!;
    public Action OnStop { get; init; } = null!;
    private State _state = State.Init;

    public void Start()
    {
        if (_state != State.Init) return;
        _state = State.Started;
        OnStart();
    }

    public void Stop()
    {
        if (_state != State.Started) return;
        _state = State.Stopped;
        OnStop();
    }

    private enum State
    {
        Init,
        Started,
        Stopped
    }
}