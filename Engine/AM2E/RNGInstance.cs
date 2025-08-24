namespace AM2E;

public sealed class RNGInstance
{
    private Random random;
    public int Seed { get; private set; }

    public void SetSeed(int seed)
    {
        random = new Random(seed);
        Seed = seed;
    }

    public RNGInstance()
    {
        Seed = 0;
        random = new Random(Seed);
    }

    public float RandomRange(float min, float max)
    {
        return min + (random.NextSingle() * (max - min));
    }

    public float Random(float max)
    {
        return random.NextSingle() * max;
    }
    
    public int RandomRange(int min, int max)
    {
        return random.Next(min, max + 1);
    }

    public int Random(int max)
    {
        return random.Next(0, max + 1);
    }

    public List<T> Shuffle<T>(List<T> list)
    {
        return list.OrderBy(_ => random.Next()).ToList();
    }

    public T Choose<T>(params T[] items)
    {
        return items[Random(items.Length - 1)];
    }
}