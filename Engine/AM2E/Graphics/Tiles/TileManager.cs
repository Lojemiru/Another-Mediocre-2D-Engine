using AM2E.Levels;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class TileManager
{
    internal readonly Tile?[,] Tiles;
    private readonly int tileSize;
    private readonly int worldX;
    private readonly int worldY;
    private readonly int tilesX;
    private readonly int tilesY;
    private int widestPlacedTile = 0;
    private int highestPlacedTile = 0;
    private readonly Level level;
    public int ImageIndex
    {
        get => (int)imageIndex;
        set
        { 
            imageIndex = value;
            WrapIndex();
        }
    }
    
    private float imageIndex = 0;
    
    public float AnimationSpeed = 0;

    public bool Randomize = false;
    public bool RepeatX = false;
    public bool RepeatY = false;
    public float ParallaxX = 0;
    public float ParallaxY = 0;

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
        this.level = level;
    }

    public void AddTile(int x, int y, Tile tile)
    {
        var tX = (x - worldX) / tileSize;
        var tY = (y - worldY) / tileSize;

        if (tX < 0 || tY < 0 || tX >= tilesX || tY >= tilesY)
            return;
        
        if (tX > widestPlacedTile)
            widestPlacedTile = tX;
        
        if (tY > highestPlacedTile)
            highestPlacedTile = tY;
        
        Tiles[tX, tY] = tile;
    }

    public Tile? GetTile(int x, int y)
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
        
        // TODO: This could stand to update widest/highest placed tiles. But I won't run into this on the current project
        // so I don't care too much right now. Will fix when it's actually a problem for somebody.
        
        Tiles[tX, tY] = null;
    }
    
    public void DeleteTiles(int x, int y, int numX, int numY)
    {
        for (var i = 0; i < numX; i++)
            for (var j = 0; j < numY; j++)
                DeleteTile(x + (i * tileSize), y + (j * tileSize));
    }
    
    private void WrapIndex()
    {
        if (imageIndex < TilesetSprite.Length && imageIndex >= 0) 
            return;

        while (imageIndex >= TilesetSprite.Length)
            imageIndex -= TilesetSprite.Length;

        while (imageIndex < 0)
            imageIndex += TilesetSprite.Length;
    }

    public void Step()
    {
        imageIndex += AnimationSpeed;
        WrapIndex();
    }

    public void Draw(SpriteBatch spriteBatch, int offsetX = 0, int offsetY = 0, int distancePastCamera = 0)
    {
        // Parallax component
        var paraX = (int)((Camera.BoundLeft - level.X) * ParallaxX);
        var paraY = (int)((Camera.BoundTop - level.Y) * ParallaxY);

        var limX = RepeatX ? int.MaxValue : widestPlacedTile + 1;
        var limY = RepeatY ? int.MaxValue : highestPlacedTile + 1;
        
        var l = Math.Clamp((Camera.BoundLeft - distancePastCamera - level.X - paraX) / 16, 0, limX);
        var u = Math.Clamp((Camera.BoundTop - distancePastCamera - level.Y - paraY) / 16, 0, limY);
        var r = Math.Clamp((Camera.BoundRight + distancePastCamera - level.X - paraX) / 16 + 1, 0, limX);
        var d = Math.Clamp((paraY + Camera.BoundBottom + distancePastCamera - level.Y - paraY) / 16 + 1, 0, limY);
        
        for (var i = l; i < r; i++)
        {
            for (var j = u; j < d; j++)
            {
                var ii = i;
                var jj = j;
                if (ii > widestPlacedTile)
                    ii %= widestPlacedTile + 1;
                
                if (jj > highestPlacedTile)
                    jj %= highestPlacedTile + 1;
                
                Tiles[ii, jj]?.Draw(spriteBatch, ImageIndex, (worldX + offsetX + paraX) + i * tileSize, (worldY + offsetY + paraY) + j * tileSize, Randomize);
            }
        }
    }
}