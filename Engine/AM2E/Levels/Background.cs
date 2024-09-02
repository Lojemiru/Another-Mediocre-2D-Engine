using System;
using AM2E.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

#region Design Notes

/*
 * Backgrounds are individual images that render with a set of repeat and parallax attributes. These two attributes also
 *      happen to be the only complicated things happening here. This struct is internal because it would not expose any
 *      useful API even if it was public; that API is presented by CompositeBackground instead.
 *
 * Parallax positioning is extremely fussy, mostly because I need it to 100% sync up with LDtk-AM2E. It mostly consists
 *      of getting the current camera position and manipulating that with the parallax factor to determine the current
 *      offset. The math is sort of self-explanatory, I'm not sure why I'm still typing here. The only people who will
 *      read this are going to be those who wonder why I'm not supporting inherently stupid things like the stretching
 *      display modes in LDtk. (The answer to that, of course, is that they look hideous and no serious game is going
 *      to make use of them so I'm not going to the trouble of supporting them.)
 *
 * Repeat drawing here is kind of a neat trick. We figure out how many times our background has to repeat in order to
 *      cover the camera area, then add one. From there, we draw that many copies of the background with an offset so
 *      that it "infinitely" scrolls while making as few draw calls as possible.
 */

#endregion

internal readonly struct Background
{
    private readonly Sprite sprite;

    private readonly float parallaxX;
    private readonly float parallaxY;
    private readonly float pivotX;
    private readonly float pivotY;
    private readonly bool repeatX;
    private readonly bool repeatY;

    public Background(LDtkBackgroundDefinition def)
    {
        var path = def.RelPath.Split('/');
        var name = path[^1].Split('.')[0];
        sprite = TextureManager.GetSprite(path[^2], name);

        parallaxX = def.ParallaxX;
        parallaxY = def.ParallaxY;
        pivotX = def.PivotX;
        pivotY = def.PivotY;
        repeatX = def.RepeatX;
        repeatY = def.RepeatY;
    }

    public void Draw(SpriteBatch spriteBatch, Level level, int layer)
    {
        // TODO: Somewhere in here, everything still looks jittery. That's a problem.
        // TODO: also, on some GPUs this does not tile perfectly! Rounding issues cause gaps between the backgrounds, sometimes.
        
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
        // TODO: As far as I can determine, THIS is what causes the jitter.
        posX += repeatX ? sprite.Width * MathF.Floor((Camera.BoundLeft - posX) / sprite.Width) : 0;
        posY += repeatY ? sprite.Height * MathF.Floor((Camera.BoundTop - posY) / sprite.Height) : 0;

        // Determine repeat counts
        var repeatCountX = repeatX ? Math.Max(MathF.Ceiling(Camera.Width / (float)sprite.Width), 1) + 1 : 1;
        var repeatCountY = repeatY ? Math.Max(MathF.Ceiling(Camera.Height / (float)sprite.Height), 1) + 1 : 1;

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