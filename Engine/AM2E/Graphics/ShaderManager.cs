

using System.Collections.Generic;
using System.IO;
using AM2E;
using AM2E.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DesktopBootstrapper.Graphics;

// TODO: make accesses/loads safe

public static class ShaderManager
{
    private static readonly Dictionary<string, Effect> effects = new();

    public static void Load(string name)
    {
        var stream = File.OpenRead(AssetManager.GetShaderPath(name));
        BinaryReader reader = new(stream);
        effects[name] = new Effect(EngineCore._graphics.GraphicsDevice,
            reader.ReadBytes((int)reader.BaseStream.Length));
    }

    public static Effect Get(string name)
    {
        return effects[name];
    }
}