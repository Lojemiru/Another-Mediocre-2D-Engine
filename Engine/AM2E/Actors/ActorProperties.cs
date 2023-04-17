using System.Collections.Generic;

namespace AM2E.Actors;

#region Design Notes

/*
 * 
 */

#endregion

// TODO: Need to implement IDisposable?
public abstract partial class Actor
{
    /// <summary>
    /// Used for quickly getting an <see cref="Actor"/> reference via UUID.
    /// </summary>
    internal static readonly Dictionary<string, Actor> AllActors = new();
    
    public bool FlippedX { get; private set; } = false;
    
    public bool FlippedY { get; private set; } = false;
    
    public readonly string ID;
    
    public bool Persistent { get; private set; } = false;
}