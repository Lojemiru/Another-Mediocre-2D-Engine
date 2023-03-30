
namespace AM2E.Levels;

public abstract class GenericLevelElement
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Layer Layer { get; internal set; }
    public Level Level { get; internal set; }
    internal bool Exists = true;

    protected GenericLevelElement(int x, int y, Layer layer)
    {
        Layer = layer;
        Layer.AddGeneric(this);
        Level = layer.Level;
        X = x;
        Y = y;
    }

    public void Destroy(bool runCustomDestroyEvent = true)
    {
        if (runCustomDestroyEvent)
            OnDestroy();
        // TODO: More robust destruction pattern for memory management etc.
        Exists = false;
        Layer.RemoveGeneric(this);
    }
    
    public virtual void OnDestroy() { }
}