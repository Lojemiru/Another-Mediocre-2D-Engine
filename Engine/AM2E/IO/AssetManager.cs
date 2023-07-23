﻿using GameContent;

namespace AM2E.IO;

/// <summary>
/// Static class for the management of assets - in particular, getting the locations and names of asset files.
/// </summary>
public static class AssetManager
{
        
    public static string GetTextureMetadataPath(PageIndex index)
    {
        return "textures/" + index + ".json";
    }
    public static string GetTexturePath(PageIndex index)
    {
        return "textures/" + index + ".png";
    }

    public static string GetShaderPath(string name)
    {
        return "shaders/" + name;
    }


    public static string GetAudioPath()
    {
        // TODO: switching for Android necessary???
        return "audio/Desktop";
    }
}