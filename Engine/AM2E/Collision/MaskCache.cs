using System.Collections.Generic;
using AM2E.Graphics;

namespace AM2E.Collision;

// TODO: Review if this is even a good design paradigm.

public static class MaskCache
{
    // TODO: Make this support mask flips!
    private static Dictionary<Sprite, Dictionary<int, bool[,]>> preciseMasks = new();

    public static bool[,] GetPrecise(Sprite sprite, int index = 0)
    {
        if (!IsCached(sprite, index))
            Add(sprite, index);
        
        return preciseMasks[sprite][index];
    }

    public static bool IsCached(Sprite sprite, int index = 0)
    {
        return preciseMasks.ContainsKey(sprite) && preciseMasks[sprite].ContainsKey(index);
    }

    public static void Add(Sprite sprite, int index = 0)
    {
        if (!preciseMasks.ContainsKey(sprite))
            preciseMasks.Add(sprite, new Dictionary<int, bool[,]>());
        
        preciseMasks[sprite][index] = sprite.ToPreciseMask(index);
    }
    
    public static void Flush()
    {
        preciseMasks = new();
    }
}