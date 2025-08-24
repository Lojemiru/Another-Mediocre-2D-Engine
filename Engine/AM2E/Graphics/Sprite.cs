using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AM2E.Graphics;

public sealed class Sprite
{
    #region Properties
    
    /// <summary>
    /// The height of this <see cref="Sprite"/> in pixels.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// The number of layers in this <see cref="Sprite"/>.
    /// </summary>
    public int Layers { get; }
    
    /// <summary>
    /// The number of frames in this <see cref="Sprite"/>.
    /// </summary>
    public int Length { get; }
    
    /// <summary>
    /// The name of this <see cref="Sprite"/>.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The origin point of this <see cref="Sprite"/>.
    /// </summary>
    public Vector2 Origin { get; }
    
    /// <summary>
    /// A collection of locations on this <see cref="Sprite"/>'s <see cref="TexturePage"/>, one entry for each frame.
    /// </summary>
    public readonly Rectangle[][] Positions;
    
    /// <summary>
    /// The <see cref="TexturePage"/> to which this <see cref="Sprite"/> belongs.
    /// </summary>
    public TexturePage TexturePage { get; set; }
    
    /// <summary>
    /// The width of this <see cref="Sprite"/> in pixels.
    /// </summary>
    public int Width { get; }
    
    #endregion
    
    
    #region Private variables
    
    /// <summary>
    /// The attach points defined for this <see cref="Sprite"/>.
    /// </summary>
    private readonly Dictionary<string, int[][]> attachPoints;
    
    /// <summary>
    /// The offsets required to draw each cropped frame correctly.
    /// </summary>
    private readonly int[][][]? cropOffsets;

    /// <summary>
    /// Static <see cref="Vector2"/> used to translate int/int positions for <see cref="SpriteBatch"/> draw calls.
    /// </summary>
    private static Vector2 drawPos;
    
    /// <summary>
    /// Static <see cref="Vector2"/> used to set the transform-respecting origin for each draw call.
    /// </summary>
    private static Vector2 origin;

    /// <summary>
    /// Static <see cref="Vector2"/> used to translate x/y scale values for <see cref="SpriteBatch"/> draw calls.
    /// </summary>
    private static Vector2 scale;
    
    /// <summary>
    /// Static <see cref="Rectangle"/> used to translate sub-rectangles for sub-rectangle draw calls.
    /// </summary>
    private static Rectangle subPos;
    
    /// <summary>
    /// Numeric constant for converting degrees to radians.
    /// </summary>
    private const double TO_RADIANS = Math.PI / 180;

    #endregion
    
    
    [JsonConstructor]
    public Sprite([JsonProperty("L")] int length, [JsonProperty("X")] int originX,
        [JsonProperty("Y")] int originY, [JsonProperty("A")] Dictionary<string, int[][]> attachPoints,
        [JsonProperty("P")] Rectangle[][] positions, [JsonProperty("W")] int width,
        [JsonProperty("H")] int height, [JsonProperty("C")] int[][][]? cropOffsets)
    {
        Length = length;
        Origin = new Vector2(originX, originY);
        this.attachPoints = attachPoints;
        Positions = positions;
        Width = width;
        Height = height;
        this.cropOffsets = cropOffsets;
        Layers = positions.Length;
    }
    
    
    #region Public Methods

    /// <summary>
    /// Draws this <see cref="Sprite"/> at the specified position and with the specified parameters.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to queue this <see cref="Sprite"/> in for drawing.</param>
    /// <param name="x">The X position at which to draw this <see cref="Sprite"/>.</param>
    /// <param name="y">The Y position at which to draw this <see cref="Sprite"/>.</param>
    /// <param name="frame">The frame of this <see cref="Sprite"/> to be drawn.
    /// Safely wraps values that are negative or longer than the <see cref="Length"/>.</param>
    /// <param name="rotation">The rotation to draw with, in degrees.</param>
    /// <param name="effects">The <see cref="SpriteEffects"/> that should be applied during drawing.</param>
    /// <param name="alpha">The alpha value that should be applied during drawing; ranges from 0 to 1 inclusive.</param>
    /// <param name="scaleX">The X-axis scale with which to draw.</param>
    /// <param name="scaleY">The Y-axis scale with which to draw.</param>
    /// <param name="color">The color with which to draw.</param>
    /// <param name="layer">The layer to draw.</param>
    /// <param name="flipOnCorner">Whether flips should apply on the corner or the center of the origin pixel.</param>
    public void Draw(SpriteBatch batch, float x, float y, int frame, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1, float scaleX = 1, float scaleY = 1, Color color = default, int layer = 0, bool flipOnCorner = true)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        
        // Constrain layer to safe indices.
        layer = Math.Clamp(layer, 0, Layers - 1);
        
        PrepareDraw(x, y, scaleX, scaleY);
        
        var currentFrame = Positions[layer][frame];
        
        // Calculate the unflipped origin...
        var originX = Origin.X;
        var originY = Origin.Y;
        
        if (cropOffsets is not null)
        {
            originX -= cropOffsets[layer][frame][0];
            originY -= cropOffsets[layer][frame][1];
        }

        // ...and then apply the origin flips. 
        origin.X = ((effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally) 
            ? (currentFrame.Width - (flipOnCorner ? 1 : 0) - originX) : originX;
        
        origin.Y = ((effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically)
            ? (currentFrame.Height - (flipOnCorner ? 1 : 0) - originY) : originY;

        scale.X = scaleX;
        scale.Y = scaleY;

        if (color == default)
            color = Color.White;
        
        // Finally, draw!
        batch.Draw(TexturePage.Texture, drawPos, currentFrame, color * alpha,
            Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), origin, scale, effects, 0);
    }

    /// <summary>
    /// Draws the given sub-rectangle of this <see cref="Sprite"/> at the specified position
    /// and with the specified parameters.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to queue this <see cref="Sprite"/> in for drawing.</param>
    /// <param name="x">The X position at which to draw this <see cref="Sprite"/>.</param>
    /// <param name="y">The Y position at which to draw this <see cref="Sprite"/>.</param>
    /// <param name="frame">The frame of this <see cref="Sprite"/> to be drawn.
    /// Safely wraps values that are negative or longer than the <see cref="Length"/>.</param>
    /// <param name="subRectangle">The sub-rectangle of this <see cref="Sprite"/> that should be drawn.</param>
    /// <param name="rotation">The rotation to draw with, in degrees.</param>
    /// <param name="effects">The <see cref="SpriteEffects"/> that should be applied during drawing.</param>
    /// <param name="alpha">The alpha value that should be applied during drawing; ranges from 0 to 1 inclusive.</param>
    /// <param name="scaleX">The X-axis scale with which to draw.</param>
    /// <param name="scaleY">The Y-axis scale with which to draw.</param>
    /// <param name="color">The color with which to draw.</param>
    /// <param name="layer">The layer to draw.</param>
    /// <param name="rectOrigin">The origin with which to draw the rectangle.</param>
    public void Draw(SpriteBatch batch, float x, float y, int frame, Rectangle subRectangle, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1, float scaleX = 1, float scaleY = 1, Color color = default, int layer = 0, Vector2? rectOrigin = null)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        
        // Constrain layer to safe indices.
        layer = Math.Clamp(layer, 0, Layers - 1);

        PrepareDraw(x, y, scaleX, scaleY);

        var pos = Positions[layer][frame];

        // Figure out the bounds of our sub-rectangle.
        subPos.X = Positions[layer][frame].X + subRectangle.X;
        subPos.Y = Positions[layer][frame].Y + subRectangle.Y;
        subPos.Width = subRectangle.Width;
        subPos.Height = subRectangle.Height;

        if (cropOffsets is not null)
        {
            subPos.X -= cropOffsets[layer][frame][0];
            subPos.Y -= cropOffsets[layer][frame][1];
        }

        var flipH = (effects & SpriteEffects.FlipHorizontally) != 0;
        var flipV = (effects & SpriteEffects.FlipVertically) != 0;

        if (subPos.X < pos.X)
        {
            var diff = (pos.X - subPos.X);
            subPos.Width -= diff;
            subPos.X = pos.X;
            
            if (!flipH)
                drawPos.X += diff;
        }
        
        var sub = (subPos.X + subPos.Width) - (pos.X + pos.Width);
        if (sub > 0)
        {
            subPos.Width -= sub;
            
            if (flipH)
                drawPos.X += sub;
        }

        if (subPos.Y < pos.Y)
        {
            var diff = (pos.Y - subPos.Y);
            subPos.Height -= diff;
            subPos.Y = pos.Y;
            
            if (!flipV)
                drawPos.Y += diff;
        }

        sub = (subPos.Y + subPos.Height) - (pos.Y + pos.Height);
        if (sub > 0)
        {
            subPos.Height -= sub;
            
            if (flipV)
                drawPos.Y += sub;
        }

        scale.X = scaleX;
        scale.Y = scaleY;
        
        if (color == default)
            color = Color.White;

        // Draw!
        batch.Draw(TexturePage.Texture, drawPos, subPos, color * alpha,
            Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), rectOrigin ?? Vector2.Zero, scale, effects, 0);
    }
    
    /// <summary>
    /// Returns a defined attach point for a given frame.
    /// </summary>
    /// <param name="name">The name of the attach point.</param>
    /// <param name="frame">The frame of the sprite that should be searched for the attach point.</param>
    /// <param name="angle">The angle the attach point should be rotated by.</param>
    /// <returns>A <see cref="Vector2"/> for the given attach point if it exists;
    /// otherwise an empty <see cref="Vector2"/>.</returns>
    public int[] GetAttachPoint(string name, int frame, float angle = 0)
    {
        if (!attachPoints.TryGetValue(name, out var value))
            return [0, 0];

        var point = (int[])value[Math.Min(frame, value.Length - 1)].Clone();
        
        point[0] -= (int)Origin.X;
        point[1] -= (int)Origin.Y;

        // If we have a modified angle, apply rotation!
        if (angle != 0)
        {
            var radAngle = angle * TO_RADIANS;
            var x = point[0];
            var y = point[1];

            var distance = Math.Round(MathHelper.PointDistance(0f, 0, x, y));
            var originalAngle = MathHelper.PointAngle(0, 0, x, y);

            point[0] = (int)Math.Round(Math.Cos(originalAngle + radAngle) * distance);
            point[1] = (int)Math.Round(Math.Sin(originalAngle + radAngle) * distance);
        }

        return point;
    }

    /// <summary>
    /// Generates a precise collision mask based on a single frame of this <see cref="Sprite"/>.
    /// </summary>
    /// <param name="frame">The frame from which the precise collision mask should be generated.</param>
    /// <param name="layer">The layer from which the collision mask should be taken.</param>
    /// <returns>A 2D bool array representing a precise collision mask.</returns>
    public bool[,] ToPreciseMask(int frame, int layer = 0)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        
        // Constrain layer to safe indices.
        layer = Math.Clamp(layer, 0, Layers - 1);

        // Copy data into array of our desired length.
        var w = Positions[layer][frame].Width;
        var h = Positions[layer][frame].Height;
        var colorArray = new Color[w * h];
        TexturePage.Texture.GetData(0, 0, Positions[layer][frame], colorArray, 0, w * h);
        
        var outputArray = new bool[w, h];
        
        // Split 1D array into a 2D array based on alpha value.
        var pos = 0;
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                outputArray[j, i] = colorArray[pos].A > 0.1;
                pos++;
            }
        }

        return outputArray;
    }
    
    #endregion
    
    private static void PrepareDraw(float x, float y, float scaleX, float scaleY)
    {
        drawPos.X = x;
        drawPos.Y = y;
        
        scale.X = scaleX;
        scale.Y = scaleY;
    }
}