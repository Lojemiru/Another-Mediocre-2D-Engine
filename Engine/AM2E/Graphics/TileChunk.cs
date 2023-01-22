using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public class TileChunk : IDrawable, IDisposable
{
    // TODO: This class is currently unused. Implement if needed for performance saves!
    
    public Tile[] Tiles;
    private RenderTarget2D texture;
    private SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
    private Rectangle renderBounds;
    public readonly int Size;

    public TileChunk(int size)
    {
        Size = size;
        
    }
    
    public void RebuildTexture()
    {
        Renderer.SetRenderTarget(texture);
        spriteBatch.Begin();
        foreach (Tile tile in Tiles)
        {
            tile.Draw(spriteBatch);
        }
        spriteBatch.End();
        Renderer.SetRenderTarget(null);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, renderBounds, Color.White);
    }

    public void Dispose()
    {
        texture?.Dispose();
    }
}