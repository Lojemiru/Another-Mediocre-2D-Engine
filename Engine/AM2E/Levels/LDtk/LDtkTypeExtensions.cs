using System.Drawing;

namespace AM2E.Levels;

public static class LDtkTypeExtensions
{
    public static Point[] ToPoint(this LDtkGridPoint[] points, Level level)
    {
        var output = new Point[points.Length];
        
        for (var i = 0; i < points.Length; i++)
        {
            output[i] = points[i].ToPoint(level);
        }

        return output;
    }

    public static Point ToPoint(this LDtkGridPoint point, Level level)
    {
        return new Point(level.X + point.X * 16, level.Y + point.Y * 16);
    }
}