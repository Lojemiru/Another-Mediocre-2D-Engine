using System;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

#region Design Notes

/*
 * This class is an aggregate of multiple Backgrounds, used to simplify their management and rendering. The only API it
 *      exposes is for setting the layer each Background should be drawing from its sprite.
 */

#endregion

public sealed class CompositeBackground
{
    private readonly Background[] backgrounds;

    private int layer = 0;
    public int Layer
    {
        get => layer;
        set => layer = Math.Max(0, value);
    }

    internal CompositeBackground(int uid)
    {
        var def = World.GetCompositeBackground(uid);
        backgrounds = new Background[def.Backgrounds.Length];

        var i = def.Backgrounds.Length - 1;
        foreach (var bgDef in def.Backgrounds)
        {
            backgrounds[i] = new Background(bgDef);
            i--;
        }
    }

    internal void Draw(SpriteBatch spriteBatch, Level level)
    {
        foreach (var bg in backgrounds)
        {
            bg.Draw(spriteBatch, level, layer);
        }
    }
}