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
}