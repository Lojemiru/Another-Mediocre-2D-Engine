using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

// Since this class is internal, we do no safety checks on positional queries as we've already done that in the TileManager.

internal sealed class TileChunk : IDrawable, IDisposable
{
    private readonly Tile[,] Tiles;
    private readonly RenderTarget2D texture;
    private static readonly SpriteBatch SpriteBatch = new(EngineCore._graphics.GraphicsDevice);
    private readonly Rectangle renderBounds;
    public readonly int TileSize;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;
    private readonly int cellsWide;

    private bool queueRebuild = true;

    internal TileChunk(int x, int y, int cellsWide, int tileSize = 16)
    {
        Tiles = new Tile[cellsWide, cellsWide];
        renderBounds = new Rectangle(x, y, cellsWide * tileSize, cellsWide * tileSize);
        TileSize = tileSize;
        X = x;
        Y = y;
        Width = cellsWide * tileSize;
        Height = cellsWide * tileSize;
        this.cellsWide = cellsWide;

        texture = new RenderTarget2D(EngineCore._graphics.GraphicsDevice, Width, Height);
    }

    internal void SetAtPosition(int x, int y, Tile tile)
    {
        SetAtCell((x - X) / TileSize, (y - Y) / TileSize, tile);
    }

    internal void SetAtCell(int cellX, int cellY, Tile tile)
    {
        Tiles[cellX, cellY] = tile;
        queueRebuild = true;
    }

    internal Tile GetAtPosition(int x, int y)
    {
        return GetAtCell((x - X) / TileSize, (y - Y) / TileSize);
    }

    internal Tile GetAtCell(int cellX, int cellY)
    {
        return Tiles[cellX, cellY];
    }

    internal void Step()
    {
        if (queueRebuild)
            Rebuild();
        
        queueRebuild = false;
    }

    private void Rebuild()
    {
        Renderer.SetRenderTarget(texture);
        EngineCore._graphics.GraphicsDevice.Clear(Color.Transparent);
        SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
        for (var i = 0; i < Tiles.GetLength(0); i++)
        {
            for (var j = 0; j < Tiles.GetLength(1); j++)
            {
                Tiles[i, j]?.Draw(SpriteBatch, i * TileSize, j * TileSize);
            }
        }
        SpriteBatch.End();
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