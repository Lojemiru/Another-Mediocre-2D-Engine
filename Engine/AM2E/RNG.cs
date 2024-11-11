using System.Collections.Generic;

namespace AM2E;

public static class RNG
{
    private static readonly RNGInstance Instance = new();
    public static int Seed 
        => Instance.Seed;

    public static void SetSeed(int seed)
        => Instance.SetSeed(seed);

    public static float RandomRange(float min, float max)
        => Instance.RandomRange(min, max);

    public static float Random(float max)
        => Instance.Random(max);

    public static int RandomRange(int min, int max)
        => Instance.RandomRange(min, max);

    public static int Random(int max)
        => Instance.Random(max);

    public static List<T> Shuffle<T>(List<T> list)
        => Instance.Shuffle(list);

    public static T Choose<T>(params T[] items)
        => Instance.Choose(items);
}