using AM2E.Graphics;

namespace AM2E.Levels;

public abstract class GenericLevelElement
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Layer Layer { get; internal set; }
    public Level Level { get; internal set; }

    protected GenericLevelElement(Layer layer)
    {
        Layer = layer;
        Layer.Add(this);
        Level = layer.Level;
    }
}