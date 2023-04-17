namespace AM2E.Actors;

#region Design Notes

/*
 * 
 */

#endregion

// TODO: Need to implement IDisposable?
public abstract partial class Actor
{
    public bool FlippedX { get; private set; } = false;
    
    public bool FlippedY { get; private set; } = false;
    
    public readonly string ID;
    
    public bool Persistent { get; private set; } = false;
}