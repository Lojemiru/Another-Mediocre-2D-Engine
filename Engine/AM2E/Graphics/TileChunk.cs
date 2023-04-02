using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class TileChunk : IDrawable, IDisposable
{
    private Tile[,] Tiles;
    private RenderTarget2D texture;
    private SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
    private Rectangle renderBounds;
    public readonly int TileSize;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;

    private bool queueRebuild = true;

    public TileChunk(int x, int y, int cellsWide, int tileSize = 16)
    {
        Tiles = new Tile[cellsWide, cellsWide];
        renderBounds = new Rectangle(x, y, cellsWide * tileSize, cellsWide * tileSize);
        TileSize = tileSize;
        X = x;
        Y = y;
        Width = cellsWide * tileSize;
        Height = cellsWide * tileSize;

        texture = new RenderTarget2D(EngineCore._graphics.GraphicsDevice, Width, Height);
    }

    public void SetAtPosition(int x, int y, Tile tile)
    {
        SetAtCell((x - X) / TileSize, (y - Y) / TileSize, tile);
    }

    public void SetAtCell(int cellX, int cellY, Tile tile)
    {
        Tiles[cellX, cellY] = tile;
        queueRebuild = true;
    }

    public Tile GetAtPosition(int x, int y)
    {
        return GetAtCell((x - X) / TileSize, (y - Y) / TileSize);
    }

    public Tile GetAtCell(int cellX, int cellY)
    {
        return Tiles[cellX, cellY];
    }

    public void Step()
    {
        if (queueRebuild)
            Rebuild();
        
        queueRebuild = false;
    }

    public void Rebuild()
    {
        Renderer.SetRenderTarget(texture);
        EngineCore._graphics.GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
        for (var i = 0; i < Tiles.GetLength(0); i++)
        {
            for (var j = 0; j < Tiles.GetLength(1); j++)
            {
                Tiles[i, j]?.Draw(spriteBatch, i * TileSize, j * TileSize);
            }
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