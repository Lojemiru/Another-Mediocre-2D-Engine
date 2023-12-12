using GameContent;
using MonoGame.Framework.Utilities;

namespace AM2E.IO;

/// <summary>
/// Static class for the management of assets - in particular, getting the locations and names of asset files.
/// </summary>
public static class AssetManager
{
    public static string GetTextureMetadataPath(string index)
    {
        return "textures/" + index + ".json";
    }
    
    public static string GetTexturePath(string index)
    {
        return "textures/" + index + ".png";
    }

    public static string GetAudioPath()
    {
        var folder = PlatformInfo.MonoGamePlatform == MonoGamePlatform.Android ? "Mobile" : "Desktop";
        return "audio/" + folder;
    }

    public static string GetFontPath(string fontName)
    {
        return "Fonts/" + fontName + ".ttf";
    }
}