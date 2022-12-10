using Windows.Win32;
using Windows.Win32.Foundation;
using NotEnoughKeys.Modules;

namespace NotEnoughKeys.Tiling;

internal class MoveWindowHandler
{
    private static HWND _hwnd;
    private static Point _point;
    private static Rectangle _rect;
    private CancellationTokenSource? _cts;

    public MoveWindowHandler(HWND hwnd)
    {
        _hwnd = hwnd;
    }

    public void Start()
    {
        _point = MouseUtils.GetMousePos();
        _rect = WindowUtils.GetWindowRect(_hwnd);
        if (!_rect.Contains(_point)) return;
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        Task.Run(() => Run(_cts.Token));
    }

    public void Stop()
    {
        NekForm.Instance.Invoke(() => GlobalLog.Debug("Stopping"));
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Run(CancellationToken cancellationToken)
    {
        var perLoop = TimeSpan.FromMilliseconds(10);
        for (TimeSpan ts = TimeSpan.Zero, tsEnd = TimeSpan.FromSeconds(5); ts < tsEnd; ts += perLoop)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                NekForm.Instance.Invoke(() => GlobalLog.Debug("Cancel requested"));
                break;
            }

            PInvoke.GetCursorPos(out var point);

            var newPos = Alignment.AlignToGrid(_rect.Location.Add(point.Sub(_point)));
            NekForm.Instance.Invoke(() => { WindowUtils.SetWindowPos(_hwnd, newPos); });
            Thread.Sleep(perLoop);
        }

        NekForm.Instance.Invoke(() => GlobalLog.Debug("Completed"));
    }
}