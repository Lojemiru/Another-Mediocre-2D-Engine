using AM2E.Levels;
using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class TileManager : IDrawable
{
    private readonly TileChunk[,] chunks;
    private readonly int chunkSizePx;
    private readonly int chunkSize;
    private readonly int tileSize;
    private readonly int worldX;
    private readonly int worldY;
    private readonly int chunksX;
    private readonly int chunksY;
    
    public TileManager(Level level, int tileSize = 16)
    {
        chunkSize = EngineCore.TileChunkSize;
        
        chunkSizePx = (tileSize * chunkSize);
        chunksX = (level.Width / chunkSizePx) + 1;
        chunksY = (level.Height / chunkSizePx) + 1;
        chunks = new TileChunk[chunksX, chunksY];
        this.tileSize = tileSize;
        worldX = level.X;
        worldY = level.Y;
    }

    public void AddTile(int x, int y, Tile tile)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;

        // Silently fail if we're trying to place a tile outside of the level bounds.
        if (chunkX < 0 || chunkY < 0 || chunkX >= chunksX || chunkY >= chunksY)
            return;

        chunks[chunkX, chunkY] ??= new TileChunk(worldX + (chunkX * chunkSizePx), 
            worldY + (chunkY * chunkSizePx), chunkSize, tileSize);

        chunks[chunkX, chunkY].SetAtPosition(x, y, tile);
    }

    public Tile GetTile(int x, int y)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;

        // Chunk invalid - return null.
        if (chunkX < 0 || chunkY < 0 || chunkX >= chunksX || chunkY >= chunksY || chunks[chunkX, chunkY] is null)
            return null;

        return chunks[chunkX, chunkY].GetAtPosition(x, y);
    }

    public void DeleteTile(int x, int y)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;
        
        // Silently fail if we're trying to delete a tile outside of the level bounds.
        if (chunkX < 0 || chunkY < 0 || chunkX >= chunksX || chunkY >= chunksY || chunks[chunkX, chunkY] is null)
            return;
        
        chunks[chunkX, chunkY].SetAtPosition(x, y, null);
    }
    
    public void DeleteTiles(int x, int y, int numX, int numY)
    {
        for (var i = 0; i < numX; i++)
            for (var j = 0; j < numY; j++)
                DeleteTile(x + (i * tileSize), y + (j * tileSize));
    }

    public void Step()
    {
        foreach (var chunk in chunks)
            chunk?.Step();
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var chunk in chunks)
        {
            chunk?.Draw(spriteBatch);
        }
    }
}