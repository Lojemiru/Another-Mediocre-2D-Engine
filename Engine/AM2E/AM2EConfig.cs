using Microsoft.Xna.Framework.Graphics;

namespace AM2E;

public sealed class AM2EConfig
{
    public bool AllowResizing = true;
    public bool IsMouseVisible = true;
    public bool UseVSync = true;
    public float TargetAspectRatio = 16 / (float)9;
    public int DefaultResolutionWidth = 1920;
    public int DefaultResolutionHeight = 1080;
    public GraphicsProfile GraphicsProfile = GraphicsProfile.HiDef;
}