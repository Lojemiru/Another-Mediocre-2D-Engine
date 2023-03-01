using System;

namespace AM2E;

public static class MathHelper
{
    /// <summary>
    /// Interpolates between two values.
    /// </summary>
    /// <param name="a">The starting value.</param>
    /// <param name="b">The target value.</param>
    /// <param name="amount">The amount to interpolate by.</param>
    /// <returns></returns>
    public static double Lerp(double a, double b, double amount)
    {
        return a + (amount * (b - a));
    }

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

    public static int PointDistance(int x1, int y1, int x2, int y2)
    {
        var x = x1 - x2;
        var y = y1 - y2;
        return (int)Math.Sqrt((x * x) + (y * y));
    }
}