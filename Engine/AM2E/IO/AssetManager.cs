using AM2E.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AM2E.IO
{
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
    }
}
