using System;
using System.Collections.Generic;
using System.Linq;

namespace AM2E;

public static class RNG
{
    private static Random random;
    public static int Seed { get; private set; }

    public static void SetSeed(int seed)
    {
        random = new Random(seed);
        Seed = seed;
    }

    static RNG()
    {
        SetSeed(0);
    }
    
    public static int RandomRange(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    public static int Random(int max)
    {
        return random.Next(0, max + 1);
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        return list.OrderBy(x => random.Next()).ToList();
    }
}