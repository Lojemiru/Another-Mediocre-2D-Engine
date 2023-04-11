using AM2E.Levels;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class TileManager : IDrawable
{
    private TileChunk[,] chunks;
    private int chunkSizePx;
    private int chunkSize;
    private int tileSize;
    private int worldX;
    private int worldY;
    
    public TileManager(Level level, int tileSize = 16, int chunkSize = 8)
    {
        chunkSizePx = (tileSize * chunkSize);
        chunks = new TileChunk[(level.Width / chunkSizePx) + 1, (level.Height / chunkSizePx) + 1];
        this.chunkSize = chunkSize;
        this.tileSize = tileSize;
        worldX = level.X;
        worldY = level.Y;
    }

    public void AddTile(int x, int y, Tile tile)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;

        chunks[chunkX, chunkY] ??= new TileChunk(worldX + (chunkX * chunkSizePx), 
            worldY + (chunkY * chunkSizePx), chunkSize, tileSize);

        chunks[chunkX, chunkY].SetAtPosition(x, y, tile);
    }

    public Tile GetTile(int x, int y)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;
        
        // TODO: Need a lot of safety checking here lol

        return chunks[chunkX, chunkY].GetAtPosition(x, y);
    }

    public void DeleteTile(int x, int y)
    {
        var chunkX = (x - worldX) / chunkSizePx;
        var chunkY = (y - worldY) / chunkSizePx;
        
        // TODO: safety checks
        
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