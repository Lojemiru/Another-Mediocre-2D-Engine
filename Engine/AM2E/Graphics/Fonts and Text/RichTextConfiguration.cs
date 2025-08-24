using FontStashSharp.RichText;

namespace AM2E.Graphics;

internal static class RichTextConfiguration
{
    internal static void ApplyConfiguration()
    {
        // Format: /i[page name:sprite name:index]
        // Index is optional
        RichTextDefaults.ImageResolver = text =>
        {
            var input = text.Split(":");
            var frame = 0;
            var layer = 0;

            Sprite sprite;

            try
            {
                sprite = TextureManager.GetSprite(input[0], input[1]);
            }
            catch
            {
                throw new ArgumentException("Invalid image embed tag: " + text);
            }

            if (input.Length > 2)
            {
                try
                {
                    frame = MathHelper.Wrap(int.Parse(input[2]), 0, sprite.Length);
                }
                catch
                {
                    throw new ArgumentException("Invalid index value in image embed tag: " + text);
                }
            }
            
            if (input.Length > 3)
            {
                try
                {
                    layer = MathHelper.Wrap(int.Parse(input[3]), 0, sprite.Layers);
                }
                catch
                {
                    throw new ArgumentException("Invalid layer value in image embed tag: " + text);
                }
            }

            return new BetterTextureFragment(sprite.TexturePage.Texture, sprite.Positions[layer][frame]);
        };
    }
}