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
}