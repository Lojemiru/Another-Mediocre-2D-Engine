using AM2E.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

// TODO: Change rendering based on pos?
// TODO: More efficient ways of tiling?

public sealed class Background
{
    private readonly Sprite sprite;
    private int repeatCountX = 1;
    private int repeatCountY = 1;

    private float parallaxX;
    private float parallaxY;
    private float pivotX;
    private float pivotY;
    private bool repeatX;
    private bool repeatY;
    private LDtkLevelBackgroundPosition pos;

    public Background(LDtkBackgroundDefinition def, Level level)
    {
        var path = def.RelPath.Split('/');
        var name = path[^1].Split('.')[0];
        sprite = TextureManager.GetSprite(path[^2], name);

        parallaxX = def.ParallaxX;
        parallaxY = def.ParallaxY;
        pivotX = def.PivotX;
        pivotY = def.PivotY;
        pos = def.Pos;
        repeatX = def.RepeatX;
        repeatY = def.RepeatY;

        if (repeatX)
            repeatCountX = level.Width / sprite.Width;

        if (repeatY)
            repeatCountY = level.Height / sprite.Height;
    }

    public void Draw(SpriteBatch spriteBatch, Level level, int layer)
    {
        var xOff = Camera.X - (int)Camera.X;
        var yOff = Camera.Y - (int)Camera.Y;
        
        var paraX = ((Camera.BoundLeft + xOff - level.X) * parallaxX);
        var paraY = ((Camera.BoundBottom + yOff - level.Y) * parallaxY);

        var offsetX = (level.Width - sprite.Width) * pivotX;
        var offsetY = (level.Height - sprite.Height) * pivotY;

        for (var i = 0; i < repeatCountX; i++)
        {
            for (var j = 0; j < repeatCountY; j++)
            {
                sprite.Draw(spriteBatch, 
                    offsetX + paraX + level.X + (i * sprite.Width), 
                    offsetY + paraY + level.Y + (j * sprite.Height), 
                    0, layer:layer);
            }
        }
    }
}