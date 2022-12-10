namespace NotEnoughKeys.Tiling;

internal static class Alignment
{
    private const int GridSize = 20;
    private const int XBorder = 8 - 1;
    private const int YBorder = 0;
    private const int Offset = 10;

    public static Point AlignToGrid(Point p) =>
        new() { X = Round(p.X) - XBorder + Offset, Y = Round(p.Y) - YBorder + Offset };


    public static (int, int) AlignToGrid2(Point p) =>
        (Round(p.X) - XBorder + Offset, Round(p.Y) - YBorder + Offset);

    private static int Round(int num)
    {
        var diff = num % GridSize;
        return diff switch
        {
            0 => num,
            >= GridSize => num + (GridSize - diff),
            _ => num - diff
        };
    }
}

public static class PointExtensions
{
    public static Point Add(this Point a, Point b) => new() { X = a.X + b.X, Y = a.Y + b.Y };
    public static Point Add(this Point a, int x, int y) => new() { X = a.X + x, Y = a.Y + y };
    public static Point Sub(this Point a, Point b) => new() { X = a.X - b.X, Y = a.Y - b.Y };
}