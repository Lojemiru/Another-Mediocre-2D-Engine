using System.Collections.Generic;
using AM2E.Graphics;

namespace AM2E.Levels;

public class Level
{
    public readonly string Name;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;
    public readonly List<Layer> Layers;
}