using AM2E.Collision;
using AM2E.Graphics;

namespace AM2E.Actors;

#region Design Notes

/*
 * 
 */

#endregion

// TODO: Need to implement IDisposable?
public abstract partial class Actor
{
    public Collider Collider { get; }
    
    public bool Exists { get; private set; } = true;
    
    public bool FlippedX { get; private set; } = false;
    
    public bool FlippedY { get; private set; } = false;
    
    public readonly string ID;
    
    public Layer Layer { get; internal set; }
    
    // TODO: Setting this manually will break things with the ActorManager... make setting it a method!
    public bool Persistent { get; set; } = false;
    
    public int X {
        get => Collider.X;
        set => Collider.X = value;
    }
    
    public int Y {
        get => Collider.Y;
        set => Collider.Y = value;
    }
}