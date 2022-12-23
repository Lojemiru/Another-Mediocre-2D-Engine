using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Graphics
{
    public class Layer
    {
        public readonly string Name;
        private readonly SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
        public readonly List<IDrawable> Drawables = new();
        public readonly int Depth;

        public Layer(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }

        public void Add(IDrawable drawable)
        {
            Drawables.Add(drawable);
        }

        public void Remove(IDrawable drawable)
        {
            Drawables.Remove(drawable);
        }

        public void Draw()
        {
            // Sort by texture to avoid constant swaps - this will save performance (particularly on tiles) but nuke depth,
            // but layers are a single depth so we don't care!!!
            spriteBatch.Begin(SpriteSortMode.Texture, transformMatrix:GameCamera.Transform);
            foreach(var drawable in Drawables)
            {
                drawable.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
