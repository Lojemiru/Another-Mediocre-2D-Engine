using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AM2E.Levels;

#region Design Notes

/*
 * There are three "layers" of level elements in AM2E. This is the lowest-level class, providing only as much
 *      functionality as is necessary for creating usable level elements. Most of the time, you will be using a
 *      ColliderBase or Actor, but there are some situations where it may be beneficial to work with a
 *      GenericLevelElement instead. While this does not implement collision logic or support the active events of the
 *      Actor, it can subscribe to global events from your EventBus or other sources. This can be particularly useful
 *      for situations in which you want to define a specific point to run an event handler at, such as running a
 *      particle burst at a given location when an event is called.
 *
 * The Dictionary of AllElements is used by this class and its two abstract children to expedite level element lookups
 *      based on UUID. Without it, we would have to resort to an iteration-based solution which would make lookups
 *      incredibly costly.
 *
 * Because we must defer GenericLevelElement creation/destruction to the end of a given game tick, the Exists() method
 *      is provided as a means of checking whether or not the provided GenericLevelElement was destroyed this tick.
 */

#endregion

public abstract class GenericLevelElement
{
    /// <summary>
    /// Used for quickly getting an <see cref="GenericLevelElement"/> reference via UUID.
    /// </summary>
    internal static readonly ConcurrentDictionary<string, GenericLevelElement> AllElements = new();
    
    public readonly string ID;
    
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Layer Layer { get; internal set; }
    public Level Level { get; internal set; }
    private bool exists = true;

    protected GenericLevelElement(int x, int y, Layer layer, string id = null)
    {
        Layer = layer;
        Layer?.AddGeneric(this);
        Level = layer?.Level;
        X = x;
        Y = y;
        ID = id ?? Guid.NewGuid().ToString();
        AllElements.TryAdd(ID, this);
        if (EngineCore.isNetworked)
        {
            EngineCore.Server?.RegisterElement(this);
        }
    }

    protected GenericLevelElement(LDtkEntityInstance entity, int x, int y, Layer layer) 
        : this(x, y, layer, entity.Iid) { }

    public void Dispose() => Dispose(false);
    
    internal virtual void Dispose(bool fromLayer)
    {
        OnDispose();
        exists = false;
        if (!fromLayer)
            Layer?.RemoveGeneric(this);
        AllElements.Remove(ID, out _);
        if (EngineCore.isNetworked)
        {
            EngineCore.Server?.DeleteObject(ID, this);
        }
    }
    
    protected virtual void OnDispose() { }

    /// <summary>
    /// Returns whether or not the given <see cref="GenericLevelElement"/> exists.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool Exists(GenericLevelElement element)
    {
        return element?.exists ?? false;
    }
    
    public static GenericLevelElement GetElement(string id)
    {
        return AllElements.ContainsKey(id) ? AllElements[id] : null;
    }
}