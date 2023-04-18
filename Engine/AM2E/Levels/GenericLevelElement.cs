
using System;
using System.Collections.Generic;
using AM2E.Actors;

namespace AM2E.Levels;

public abstract class GenericLevelElement
{
    /// <summary>
    /// Used for quickly getting an <see cref="GenericLevelElement"/> reference via UUID.
    /// </summary>
    internal static readonly Dictionary<string, GenericLevelElement> AllElements = new();
    
    // TODO: Moving ID here has done VERY BAD THINGS to instantiation from levels. IDs are not maintained from levels anymore!
    public readonly string ID;
    
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Layer Layer { get; internal set; }
    public Level Level { get; internal set; }
    private bool exists = true;

    protected GenericLevelElement(int x, int y, Layer layer, string id = null)
    {
        Layer = layer;
        Layer.AddGeneric(this);
        Level = layer.Level;
        X = x;
        Y = y;
        ID = id ?? Guid.NewGuid().ToString();
        AllElements.Add(ID, this);
    }

    public void Destroy(bool runCustomDestroyEvent = true)
    {
        if (runCustomDestroyEvent)
            OnDestroy();
        // TODO: More robust destruction pattern for memory management etc.
        exists = false;
        Layer.RemoveGeneric(this);
        AllElements.Remove(ID);
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
    
    public static GenericLevelElement GetElement(string id)
    {
        return AllElements.ContainsKey(id) ? AllElements[id] : null;
    }
    
    public virtual void OnDestroy() { }
}