using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace AM2E.Graphics;

public static class FontManager
{
    private static readonly Dictionary<string, FontSystem> fontSystems = new();

    public static FontSystem CreateFontSystem(string name, string pathToTTF)
    {
        var system = new FontSystem();
        system.AddFont(File.ReadAllBytes(pathToTTF));
        fontSystems.Add(name, system);

        return system;
    }

    public static FontSystem CreateFontSystem(string name, string pathToTTF, FontSystemSettings settings)
    {
        var system = new FontSystem(settings);
        system.AddFont(File.ReadAllBytes(pathToTTF));
        fontSystems.Add(name, system);

        return system;
    }

    public static FontSystem GetFontSystem(string name)
    {
        return fontSystems[name];
    }

    public static SpriteFontBase GetFont(string systemName, int fontSize)
    {
        return fontSystems[systemName].GetFont(fontSize);
    }

    public static void FixedGlyphRenderer(int threshold, byte[] input, byte[] output, GlyphRenderOptions options)
    {
        // https://github.com/FontStashSharp/FontStashSharp/releases/tag/1.3.4 has context for all this nonsense
        
        if (threshold is < 0 or > 254)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be a value between 0 and 254 (inclusive).");
        
        var size = options.Size.X * options.Size.Y;

        for (var i = 0; i < size; i++)
        {
            var c = input[i];
            var ci = i * 4;

            if (c < threshold)
            {
                output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 0;
            } 
            else
            {
                output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 255;
            }
        }
    }
}