using System;

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
    /// Returns the integer distance between two points.
    /// </summary>
    /// <param name="x1">X position of the first point.</param>
    /// <param name="y1">Y position of the first point.</param>
    /// <param name="x2">X position of the second point.</param>
    /// <param name="y2">Y position of the second point.</param>
    /// <returns>The integer distance between the two supplied points.</returns>
    public static int PointDistance(int x1, int y1, int x2, int y2)
    {
        var x = x1 - x2;
        var y = y1 - y2;
        return (int)Math.Sqrt((x * x) + (y * y));
    }
    
    public static bool DoLinesIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        var t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) 
                / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        var u = ((x1 - x3) * (y1 - y2) - (y1 - y3) * (x1 - x2))
                / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

        return (u is >= 0 and <= 1 && t is >= 0 and <= 1);
    }
}