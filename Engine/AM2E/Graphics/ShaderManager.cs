
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class ShaderManager
{
    private static readonly Dictionary<string, Effect> Effects = new();

    internal static void LoadAll()
    {
        var folderInfo = new DirectoryInfo("shaders");
        
        foreach (var file in folderInfo.GetFiles())
        {
            var stream = File.OpenRead(file.FullName);
            BinaryReader reader = new(stream);
            Effects[file.Name] = new Effect(EngineCore._graphics.GraphicsDevice, 
                reader.ReadBytes((int)reader.BaseStream.Length));
        }
    }

    public static Effect Get(string name)
    {
        if (!Effects.ContainsKey(name))
            throw new ArgumentOutOfRangeException(nameof(name), "Shader \"" + name + "\" does not exist!");
        
        return Effects[name];
    }
}