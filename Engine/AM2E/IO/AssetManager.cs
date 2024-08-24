using MonoGame.Framework.Utilities;

namespace AM2E.IO;

/// <summary>
/// Static class for the management of assets - in particular, getting the locations and names of asset files.
/// </summary>
public static class AssetManager
{
    private static string texturesFolder = "Textures";
    private static string audioFolder = "Audio";
    private static string fontFolder = "Fonts";
    private static string shadersFolder = "Shaders";
    public static void SetTexturePath(string path)
    {
        texturesFolder = path;
    }

    public static void SetAudioPath(string path)
    {
        audioFolder = path;
    }

    public static void SetFontPath(string path)
    {
        fontFolder = path;
    }

    public static void SetShaderPath(string path)
    {
        shadersFolder = path;
    }

    public static string GetTextureMetadataPath(string index)
    {
        return $"{texturesFolder}/{index}.json";
    }

    public static string GetTexturePath(string index)
    {
        return $"{texturesFolder}/{index}.png";
    }

    public static string GetAudioPath()
    {
        var folder = PlatformInfo.MonoGamePlatform == MonoGamePlatform.Android ? "Mobile" : "Desktop";
        return $"{audioFolder}/{folder}";
    }

    public static string GetFontPath(string fontName)
    {
        return $"{fontFolder}/{fontName}.ttf";
    }

    public static string GetShadersPath()
    {
        return $"{shadersFolder}";
    }
}