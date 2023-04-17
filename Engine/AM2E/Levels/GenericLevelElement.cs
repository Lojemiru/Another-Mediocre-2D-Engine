
namespace AM2E.Levels;

public abstract class GenericLevelElement
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Layer Layer { get; internal set; }
    public Level Level { get; internal set; }
    private bool exists = true;

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
        exists = false;
        Layer.RemoveGeneric(this);
    }

    /// <summary>
    /// Returns whether or not the given <see cref="GenericLevelElement"/> exists.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool Exists(GenericLevelElement element)
    {
        return element?.exists ?? false;
    }
    
    public virtual void OnDestroy() { }
}