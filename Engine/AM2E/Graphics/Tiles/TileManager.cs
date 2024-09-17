using AM2E.Levels;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace AM2E.Graphics;

public sealed class TileManager : IDrawable
{
    internal readonly Tile[,] Tiles;
    private readonly int tileSize;
    private readonly int worldX;
    private readonly int worldY;
    private readonly int tilesX;
    private readonly int tilesY;

    public readonly Sprite TilesetSprite;

    public TileManager(Level level, Sprite tileset, int tileSize = 16)
    {
        TilesetSprite = tileset;
        this.tileSize = tileSize;
        worldX = level.X;
        worldY = level.Y;
        tilesX = (level.Width / tileSize) + 1;
        tilesY = (level.Height / tileSize) + 1;
        Tiles = new Tile[tilesX, tilesY];
    }

    public void AddTile(int x, int y, Tile tile)
    {
        var tX = (x - worldX) / tileSize;
        var tY = (y - worldY) / tileSize;

        if (tX < 0 || tY < 0 || tX >= tilesX || tY >= tilesY)
            return;
        
        Tiles[tX, tY] = tile;
    }

    public Tile GetTile(int x, int y)
    {
        var tX = (x - worldX) / tileSize;
        var tY = (y - worldY) / tileSize;
        
        if (tX < 0 || tY < 0 || tX >= tilesX || tY >= tilesY)
            return null;
        
        return Tiles[tX, tY];
    }

    public void DeleteTile(int x, int y)
    {
        var tX = (x - worldX) / tileSize;
        var tY = (y - worldY) / tileSize;
        
        if (tX < 0 || tY < 0 || tX >= tilesX || tY >= tilesY)
            return;
        
        Tiles[tX, tY] = null;
    }
    
    public void DeleteTiles(int x, int y, int numX, int numY)
    {
        for (var i = 0; i < numX; i++)
            for (var j = 0; j < numY; j++)
                DeleteTile(x + (i * tileSize), y + (j * tileSize));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (var i = 0; i < Tiles.GetLength(0); i++)
        {
            for (var j = 0; j < Tiles.GetLength(1); j++)
            {
                Tiles[i, j]?.Draw(spriteBatch, worldX + i * tileSize, worldY + j * tileSize);
            }
        }
    }
}