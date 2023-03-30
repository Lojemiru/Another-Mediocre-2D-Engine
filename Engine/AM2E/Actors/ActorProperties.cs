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

    // TODO: Setting this manually will break things with the ActorManager... make setting it a method!
    public bool Persistent { get; set; } = false;
}