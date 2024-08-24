
using System;
using System.Collections.Generic;
using System.IO;
using AM2E.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class ShaderManager
{
    private static readonly Dictionary<string, Effect> Effects = new();
    private static bool loaded = false;

    public static void Load()
    {
        if (loaded)
            throw new ContentLoadException("Shaders have already been loaded! Please call Unload() first.");
        
        var folderInfo = new DirectoryInfo(AssetManager.GetShadersPath());
        
        foreach (var file in folderInfo.GetFiles())
        {
            var stream = File.OpenRead(file.FullName);
            BinaryReader reader = new(stream);
            Effects[file.Name] = new Effect(EngineCore._graphics.GraphicsDevice, 
                reader.ReadBytes((int)reader.BaseStream.Length));
        }

        loaded = true;
    }

    public static void Unload()
    {
        Effects.Clear();
        GC.Collect();
        loaded = false;
    }

    public static Effect Get(string name)
    {
        if (!loaded)
            throw new TypeUnloadedException("Shaders have not been loaded! Please call ShaderManager.Load() before accessing any shaders.");
        
        if (!Effects.TryGetValue(name, out var value))
            throw new ArgumentOutOfRangeException(nameof(name), "Shader \"" + name + "\" does not exist!");
        
        return value;
    }
}