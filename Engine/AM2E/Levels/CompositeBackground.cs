using AM2E.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Levels;

public sealed class CompositeBackground
{
    private readonly Background[] backgrounds;
    
    public CompositeBackground(int uid, Level level)
    {
        var def = World.GetCompositeBackground(uid);
        backgrounds = new Background[def.Backgrounds.Length];

        var i = def.Backgrounds.Length - 1;
        foreach (var bgDef in def.Backgrounds)
        {
            backgrounds[i] = new Background(bgDef, level);
            i--;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Level level)
    {
        foreach (var bg in backgrounds)
        {
            bg.Draw(spriteBatch, level);
        }
    }
}