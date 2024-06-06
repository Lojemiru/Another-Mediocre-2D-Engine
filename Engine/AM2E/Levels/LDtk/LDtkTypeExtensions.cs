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
}