using Windows.Win32.Foundation;
using NotEnoughKeys.Modules;

namespace NotEnoughKeys.Tiling;

internal class ResizeWindowHandler
{
    private static HWND _hwnd;
    private static Point _point;
    private static Rectangle _rect;
    private CancellationTokenSource? _cts;
    private Quadrant _quadrant;

    public ResizeWindowHandler(HWND hwnd)
    {
        _hwnd = hwnd;
    }

    public void Start()
    {
        _point = MouseUtils.GetMousePos();
        _rect = WindowUtils.GetWindowRect(_hwnd);
        if (!_rect.Contains(_point)) return;
        _quadrant = GetQuadrant(_point, _rect);
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
        var perLoop = TimeSpan.FromMilliseconds(6); // ~165FPS
        for (TimeSpan ts = TimeSpan.Zero, tsEnd = TimeSpan.FromSeconds(5); ts < tsEnd; ts += perLoop)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                NekForm.Instance.Invoke(() => GlobalLog.Debug("Cancel requested"));
                break;
            }

            var delta = MouseUtils.GetMousePos().Sub(_point);

            var (left, right, top, bottom) = (_rect.Left, _rect.Right, _rect.Top, _rect.Bottom);

            switch (_quadrant)
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
            if (_rect.Right != right)
                right += xAlign;
            if (_rect.Bottom != bottom)
                bottom += yAlign;

            var newRect = Rectangle.FromLTRB(left, top, right, bottom);

            NekForm.Instance.Invoke(() => WindowUtils.SetWindowRect(_hwnd, newRect with
            {
                Width = Math.Max(newRect.Width, 100),
                Height = Math.Max(newRect.Height, 100)
            }));

            Thread.Sleep(perLoop);
        }

        NekForm.Instance.Invoke(() => GlobalLog.Debug("Completed"));
    }

    private Quadrant GetQuadrant(Point point, Rectangle rectangle)
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

    private enum Quadrant
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}