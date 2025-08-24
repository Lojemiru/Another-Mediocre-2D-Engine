namespace AM2E;

public static class MathHelper
{
    /// <summary>
    /// Interpolates between two values.
    /// </summary>
    /// <param name="a">The starting value.</param>
    /// <param name="b">The target value.</param>
    /// <param name="amount">The amount to interpolate between the two values.</param>
    /// <returns>The interpolated value.</returns>
    public static double Lerp(double a, double b, double amount)
    {
        return a + (amount * (b - a));
    }

    /// <summary>
    /// Interpolates between two values.
    /// </summary>
    /// <param name="a">The starting value.</param>
    /// <param name="b">The target value.</param>
    /// <param name="amount">The amount to interpolate between the two values.</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float a, float b, float amount)
    {
        return a + (amount * (b - a));
    }
    
    /// <summary>
    /// Wraps the supplied value within the specified bounds.
    /// </summary>
    /// <param name="value">The input value to wrap.</param>
    /// <param name="min">The minimum value, inclusive.</param>
    /// <param name="max">The maximum value, exclusive.</param>
    /// <returns>The wrapped value.</returns>
    // Thanks to Juju Adams!
    public static int Wrap(int value, int min, int max)
    {
        var mod = (value - min) % (max - min);
        return mod + (mod < 0 ? max : min);
    }
    
    /// <summary>
    /// Splits the contents of the input 1D array into the supplied 2D array.
    /// </summary>
    /// <param name="input">The 1D array to split.</param>
    /// <param name="output">The 2D array to be populated.</param>
    /// <typeparam name="T">The array type.</typeparam>
    public static void SplitArrayTo2D<T>(T[] input, T[,] output)
    {
        var pos = 0;
        for (var i = 0; i < output.GetLength(0); i++)
        {
            for (var j = 0; j < output.GetLength(1); j++)
            {
                output[i, j] = input[pos];
                pos++;
            }
        }
    }
    
    /// <summary>
    /// Returns the distance between two points.
    /// </summary>
    /// <param name="x1">X position of the first point.</param>
    /// <param name="y1">Y position of the first point.</param>
    /// <param name="x2">X position of the second point.</param>
    /// <param name="y2">Y position of the second point.</param>
    /// <returns>The distance between the two supplied points.</returns>
    public static float PointDistance(float x1, float y1, float x2, float y2)
    {
        var x = x2 - x1;
        var y = y2 - y1;
        return MathF.Sqrt((x * x) + (y * y));
    }

    private static readonly float PointAngleSubtractor = MathF.Atan2(0, 0);
    public static float PointAngle(float x1, float y1, float x2, float y2)
    {
        return (MathF.Atan2(y2 - y1, x2 - x1) - PointAngleSubtractor);
    }

    public static float ToRadians(float degrees)
    {
        return degrees * MathF.PI / 180;
    }

    public static float ToDegrees(float radians)
    {
        return radians * 180 / MathF.PI;
    }
    
    public static float LineComponentX(float radians, float length)
    {
        return length * MathF.Cos(radians);
    }

    public static float LineComponentY(float radians, float length)
    {
        return length * MathF.Sin(radians);
    }

    public static bool DoLinesIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        var x12 = x1 - x2;
        var x13 = x1 - x3;
        var x34 = x3 - x4;
        var y12 = y1 - y2;
        var y13 = y1 - y3;
        var y34 = y3 - y4;
        var denominator = x12 * y34 - y12 * x34;
        var t = (x13 * y34 - y13 * x34) / denominator;
        var u = (x13 * y12 - y13 * x12) / denominator;
		
        return (u is >= 0 and <= 1 && t is >= 0 and <= 1) || PointIsOnLine(x1, y1, x3, y3, x4, y4) || PointIsOnLine(x2, y2, x3, y3, x4, y4) || PointIsOnLine(x3, y3, x1, y1, x2, y2) || PointIsOnLine(x4, y4, x1, y1, x2, y2);
    }
	
    public static bool PointIsOnLine(float x, float y, float x1, float y1, float x2, float y2)
    {
        if (x < Math.Min(x1, x2) || x > Math.Max(x1, x2) || y < Math.Min(y1, y2) || y > Math.Max(y1, y2))
            return false;
		
        return (x - x1) * (y2 - y1) - (y - y1) * (x2 - x1) == 0;
    }
    
    public static bool IsApproximatelyZero(double val, double precision = 0.1)
    {
        return (Math.Abs(val) < precision);
    }

    public static float RoundToZero(float val, float precision = 0.0001f)
    {
        return (Math.Abs(val) < precision) ? 0 : val;
    }
}