using System.Collections.Generic;

namespace AM2E.Actors;

#region Design Notes

/*
 * 
 */

#endregion

public abstract partial class Actor
{
    public bool FlippedX { get; private set; } = false;
    
    public bool FlippedY { get; private set; } = false;

    public bool Persistent { get; private set; } = false;
}