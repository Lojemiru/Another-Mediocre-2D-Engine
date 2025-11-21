using AM2E.Actors;
using AM2E.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

public sealed class Level
{
    public readonly string Name;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;
    public readonly Dictionary<string, Layer> Layers = new();
    public readonly string Iid;
    public bool Active { get; internal set; } = false;
    public readonly CompositeBackground? Background;
    private readonly SpriteBatch bgBatch;
    private readonly SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);

    public readonly LDtkFieldInstance[] FieldInstances;

    public Level(LDtkLevelInstance level)
    {
        Name = level.Identifier;
        X = level.WorldX;
        Y = level.WorldY;
        Width = level.PxWid;
        Height = level.PxHei;
        Iid = level.Iid;
        if (level.BackgroundUid is not null)
        {
            Background = new CompositeBackground((int)level.BackgroundUid);
            bgBatch = new SpriteBatch(EngineCore._graphics.GraphicsDevice);
        }

        FieldInstances = level.FieldInstances;
    }
    public Level(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public Layer AddLayer(string name)
    {
        if (Layers.ContainsKey(name))
            throw new ArgumentException("A layer with the specified name \"" + name + "\" already exists in level \"" + Name + "\"");
        
        Layers.Add(name, new Layer(name, this));

        return Layers[name];
    }

    public Layer? GetLayer(string name)
    {
        try
        {
            return Layers[name];
        }
        catch
        {
            return null;
        }
    }

    public void Add(string layerName, Tile tile, int x, int y)
    {
        if (!Layers.TryGetValue(layerName, out var value))
            throw new ArgumentException("No layer with the specified name \"" + layerName + "\" exists in level \"" + Name + "\"");
        
        value.AddTile(x, y, tile);
    }
    

    public void Add(string layerName, IDrawable drawable)
    {
        if (!Layers.TryGetValue(layerName, out var value))
            throw new ArgumentException("No layer with the specified name \"" + layerName + "\" exists in level \"" + Name + "\"");
        
        value.Add(drawable);
    }
    
    public void Add(string layerName, Actor actor)
    {
        Layers[layerName].Add(actor);
    }
    
    public void Add(string layerName, GenericLevelElement genericLevelElement)
    {
        Layers[layerName].Add(genericLevelElement);
    }

    internal void Draw()
    {
        if (Background is not null)
        {
            bgBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp,
                transformMatrix: Camera.Transform, blendState: BlendState.AlphaBlend);

            Background.Draw(bgBatch, this);

            bgBatch.End();
        }
        
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp,
            transformMatrix: Camera.Transform, blendState: BlendState.AlphaBlend);

        foreach (var layer in Layers.Values)
        {
            //var s = new Stopwatch();
            //s.Start();
            layer.Draw(spriteBatch);
            //s.Stop();
            //Console.WriteLine($"LayerName: {layer.Name}, Time to render: {s.ElapsedMilliseconds}, TTR (ticks): {s.ElapsedTicks}");
        }
        
        spriteBatch.End();
    }

    internal void PreTick()
    {
        foreach (var layer in Layers.Values)
        {
            layer.PreTick();
        }
    }
    
    internal void Tick()
    {
        foreach (var layer in Layers.Values)
        {
            layer.Tick();
        }
    }

    internal void PostTick()
    {
        foreach (var layer in Layers.Values)
        {
            layer.PostTick();
        }
        
        Background?.Step();
    }

    internal void Activate()
    {
        Active = true;

        foreach (var layer in Layers.Values)
            layer.InTick = true;

        foreach (var layer in Layers.Values)
            layer.Activate();

        foreach (var layer in Layers.Values)
            layer.HandleAdditionAndRemoval();
        
        foreach (var layer in Layers.Values)
            layer.InTick = false;
    }

    internal void Deactivate()
    {
        Active = false;
        
        foreach (var layer in Layers.Values)
            layer.InTick = true;
        
        foreach (var layer in Layers.Values)
            layer.Deactivate();

        foreach (var layer in Layers.Values)
            layer.HandleAdditionAndRemoval();
        
        foreach (var layer in Layers.Values)
            layer.InTick = false;
    }

    internal void Dispose()
    {
        foreach (var layer in Layers.Values)
        {
            layer.Dispose();
        }
    }

    public static event Action<Level> PreLoadHook = _ => { };
    
    internal void PreLoad()
        => PreLoadHook(this);

    public static event Action<Level> PostLoadHook = _ => { };
    
    internal void PostLoad()
        => PostLoadHook(this);

    public static event Action<Level> AsyncPostLoadHook = _ => { };
    
    internal void AsyncPostLoad()
        => AsyncPostLoadHook(this);
    
    public static event Action<Level> PreUnloadHook = _ => { };
    
    internal void PreUnload()
        => PreUnloadHook(this);

    public static event Action<Level> PostUnloadHook = _ => { };
    
    internal void PostUnload()
        => PostUnloadHook(this);
    
    public dynamic? GetFieldInstance(string identifier)
        => FieldInstances.GetFieldInstance(identifier);

    public T? GetFieldInstance<T>(string identifier) where T : struct, Enum
        => FieldInstances.GetFieldInstance<T>(identifier);

    public T[]? GetFieldInstanceArray<T>(string identifier)
        => FieldInstances.GetFieldInstanceArray<T>(identifier);
}