using Windows.Win32.Foundation;
using NotEnoughKeys.Handlers;
using NotEnoughKeys.Modules;
using NotEnoughKeys.Registry;

namespace NotEnoughKeys.Tiling;

internal class MoveWindowHandler : IDisposable
{
    private static HWND _hwnd;
    private CancellationTokenSource? _cts;
    private readonly HookRegister2 _hookRegister;
    private readonly MouseStateInterceptHandler _mouseStateIntercept;

    public MoveWindowHandler(HWND hwnd)
    {
        _hwnd = hwnd;
        _hookRegister = new HookRegister2();
        _mouseStateIntercept = new MouseStateInterceptHandler();
    }

    public void Start()
    {
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _hookRegister.Register(_mouseStateIntercept);
        Task.Run(() => Run(_cts.Token));
    }

    public void Stop()
    {
        NekForm.Instance.Invoke(() => GlobalLog.Debug("Stopping"));
        _cts?.Cancel();
        _hookRegister.Unregister();
    }

    public void Run(CancellationToken cancellationToken)
    {
        // ReSharper disable once TooWideLocalVariableScope | false positive; would break within the loop
        var startPoint = new Point();
        // ReSharper disable once TooWideLocalVariableScope | false positive; would break within the loop
        var rect = new Rectangle();
        var wasLeftDown = false;
        var wasRightDown = false;
        var moveEnabled = false;
        var resizeEnabled = false;
        var quadrant = Quadrant.BottomRight;

        var perLoop = TimeSpan.FromMilliseconds(10);
        for (TimeSpan ts = TimeSpan.Zero, tsEnd = TimeSpan.FromSeconds(5); ts < tsEnd; ts += perLoop)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                NekForm.Instance.Invoke(() => GlobalLog.Debug("Cancel requested"));
                _hookRegister.Unregister();
                break;
            }

            var isLeftDown = _mouseStateIntercept.LeftMouseDown;
            if (isLeftDown != wasLeftDown)
            {
                if (isLeftDown)
                {
                    startPoint = MouseUtils.GetMousePos();
                    rect = WindowUtils.GetWindowRect(_hwnd);
                    moveEnabled = rect.Contains(startPoint);
                }
                else
                {
                    moveEnabled = false;
                }

                wasLeftDown = isLeftDown;
            }

            var isRightDown = _mouseStateIntercept.RightMouseDown;
            if (!isLeftDown && (isRightDown != wasRightDown))
            {
                if (isRightDown)
                {
                    startPoint = MouseUtils.GetMousePos();
                    rect = WindowUtils.GetWindowRect(_hwnd);
                    resizeEnabled = rect.Contains(startPoint);
                    quadrant = WindowResizeUtils.GetQuadrant(startPoint, rect);
                }
                else
                {
                    resizeEnabled = false;
                }

                wasRightDown = isRightDown;
            }


            if (moveEnabled)
            {
                var delta = MouseUtils.GetMousePos().Sub(startPoint);
                var newPos = Alignment.AlignToGrid(rect.Location.Add(delta));
                NekForm.Instance.Invoke(() => { WindowUtils.SetWindowPos(_hwnd, newPos); });
            }
            else if (resizeEnabled)
            {
                var delta = MouseUtils.GetMousePos().Sub(startPoint);
                var newRect = WindowResizeUtils.NewRect(quadrant, rect, delta);
                NekForm.Instance.Invoke(() => WindowUtils.SetWindowRect(_hwnd, newRect));
            }

            Thread.Sleep(perLoop);
        }

        NekForm.Instance.Invoke(() => GlobalLog.Debug("Completed"));
        _hookRegister.Unregister();
    }


    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _hookRegister.Dispose();
    }
}

internal static class WindowResizeUtils
{
    public static Quadrant GetQuadrant(Point point, Rectangle rectangle)
    {
        var center = new Point { X = rectangle.X + rectangle.Width / 2, Y = rectangle.Y + rectangle.Height / 2 };
        if (point.X < center.X && point.Y < center.Y)
            return Quadrant.TopLeft;
        if (point.X > center.X && point.Y < center.Y)
            return Quadrant.TopRight;
        if (point.X < center.X && point.Y > center.Y)
            return Quadrant.BottomLeft;
        return Quadrant.BottomRight;
    }


    public static Rectangle NewRect(Quadrant quadrant, Rectangle rect, Point delta)
    {
        var (left, right, top, bottom) = (rect.Left, rect.Right, rect.Top, rect.Bottom);

        switch (quadrant)
        {
            case Quadrant.TopRight:
                (right, top) = Alignment.AlignToGrid2(delta.Add(right, top));
                break;
            case Quadrant.BottomLeft:
                (left, bottom) = Alignment.AlignToGrid2(delta.Add(left, bottom));
                break;
            case Quadrant.TopLeft:
                (left, top) = Alignment.AlignToGrid2(delta.Add(left, top));
                break;
            case Quadrant.BottomRight:
            default:
                (right, bottom) = Alignment.AlignToGrid2(delta.Add(right, bottom));
                break;
        }

        const int xAlign = 14; // Alignment.XBorder * 2
        const int yAlign = 7; // Alignment.XBorder * 1
        if (rect.Right != right)
            right += xAlign;
        if (rect.Bottom != bottom)
            bottom += yAlign;

        var unsafeSizeRect = Rectangle.FromLTRB(left, top, right, bottom);
        return unsafeSizeRect with
        {
            Width = Math.Max(unsafeSizeRect.Width, 100),
            Height = Math.Max(unsafeSizeRect.Height, 100)
        };
    }
}

internal enum Quadrant
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}