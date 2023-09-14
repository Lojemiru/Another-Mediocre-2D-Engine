using System;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E;

public sealed class AM2EConfig
{
    public bool AllowResizing = true;
    public bool IsMouseVisible = true;
    public bool UseVSync = true;
    public bool PreferMultiSampling = false;
    public float TargetAspectRatio = 16 / (float)9;
    public int DefaultResolutionWidth = 1920;
    public int DefaultResolutionHeight = 1080;
    public int TileChunkSize = 8;
    public GraphicsProfile GraphicsProfile = GraphicsProfile.HiDef;
    public Type InputEnum;

    public AM2EConfig(Type inputEnum)
    {
        InputEnum = inputEnum;
    }
}