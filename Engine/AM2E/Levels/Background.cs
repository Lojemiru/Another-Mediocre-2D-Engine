using System;
using AM2E.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

// TODO: Change rendering based on pos?

public sealed class Background
{
    private readonly Sprite sprite;

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
    }

    public void Draw(SpriteBatch spriteBatch, Level level, int layer)
    {
        // Fractional camera offset
        var xOff = Camera.X - (int)Camera.X;
        var yOff = Camera.Y - (int)Camera.Y;
        
        // Parallax component
        var paraX = ((Camera.BoundLeft + xOff - level.X) * parallaxX);
        var paraY = ((Camera.BoundBottom + yOff - level.Y) * parallaxY);

        // Offset based on pivot
        var offsetX = (level.Width - sprite.Width) * pivotX;
        var offsetY = (level.Height - sprite.Height) * pivotY;

        // Base position
        var posX = offsetX + paraX + level.X;
        var posY = offsetY + paraY + level.Y;

        // Adjust position for repeat drawing
        posX += repeatX ? sprite.Width * ((Camera.BoundLeft - (int)posX) / sprite.Width) : 0;
        posY += repeatY ? sprite.Height * ((Camera.BoundTop - (int)posY) / sprite.Height) : 0;

        // Determine repeat counts
        var repeatCountX = repeatX ? Math.Max(Camera.Width / sprite.Width, 1) + 1 : 1;
        var repeatCountY = repeatY ? Math.Max(Camera.Height / sprite.Height, 1) + 1 : 1;

        // Loop over repeat counts and actually draw
        for (var i = 0; i < repeatCountX; i++)
        {
            for (var j = 0; j < repeatCountY; j++)
            {
                sprite.Draw(spriteBatch,
                    posX + (sprite.Width * i),
                    posY + (sprite.Height * j), 
                    0, layer: layer);
            }
        }
    }
}