using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AM2E.Graphics;

public sealed class Sprite
{
    #region Properties
    
    /// <summary>
    /// The height of this <see cref="Sprite"/> in pixels.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// The number of frames in this <see cref="Sprite"/>.
    /// </summary>
    public int Length { get; }
    
    /// <summary>
    /// The origin point of this <see cref="Sprite"/>.
    /// </summary>
    public Vector2 Origin { get; }
    
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
    private readonly int[][] cropOffsets;
    
    /// <summary>
    /// Static <see cref="Vector2"/> used to translate int/int positions for <see cref="SpriteBatch"/> draw calls.
    /// </summary>
    private static Vector2 drawPos;
    
    /// <summary>
    /// Static <see cref="Vector2"/> used to set the transform-respecting origin for each draw call.
    /// </summary>
    private static Vector2 origin;
    
    /// <summary>
    /// A collection of locations on this <see cref="Sprite"/>'s <see cref="TexturePage"/>, one entry for each frame.
    /// </summary>
    private readonly Rectangle[] positions;
    
    /// <summary>
    /// Static <see cref="Vector2"/> used to translate x/y scale values for <see cref="SpriteBatch"/> draw calls.
    /// </summary>
    private static Vector2 scale;
    
    /// <summary>
    /// Static <see cref="Rectangle"/> used to translate sub-rectangles for sub-rectangle draw calls.
    /// </summary>
    private static Rectangle subPos;

    #endregion
    
    
    [JsonConstructor]
    public Sprite([JsonProperty("length")] int length, [JsonProperty("originX")] int originX,
        [JsonProperty("originY")] int originY, [JsonProperty("attachPoints")] Dictionary<string, int[][]> attachPoints,
        [JsonProperty("positions")] Rectangle[] positions, [JsonProperty("width")] int width,
        [JsonProperty("height")] int height, [JsonProperty("cropOffsets")] int[][] cropOffsets)
    {
        Length = length;
        Origin = new Vector2(originX, originY);
        this.attachPoints = attachPoints;
        this.positions = positions;
        Width = width;
        Height = height;
        this.cropOffsets = cropOffsets;
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
    public void Draw(SpriteBatch batch, float x, float y, int frame, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1, float scaleX = 1, float scaleY = 1, Color color = default)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        
        PrepareDraw(x, y, scaleX, scaleY);
        
        var currentFrame = positions[frame];
        
        // Calculate the unflipped origin...
        var originX = Origin.X - cropOffsets[frame][0];
        var originY = Origin.Y - cropOffsets[frame][1];

        // ...and then apply the origin flips. 
        origin.X = ((effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally) 
            ? (currentFrame.Width - 1 - originX) : originX;
        
        origin.Y = ((effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically)
            ? (currentFrame.Height - 1 - originY) : originY;

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
    public void Draw(SpriteBatch batch, float x, float y, int frame, Rectangle subRectangle, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1, float scaleX = 1, float scaleY = 1, Color color = default)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);

        PrepareDraw(x, y, scaleX, scaleY);

        // Figure out the bounds of our sub-rectangle.
        subPos.X = positions[frame].X + subRectangle.X - cropOffsets[frame][0];
        subPos.Y = positions[frame].Y + subRectangle.Y - cropOffsets[frame][1];
        subPos.Width = subRectangle.Width;
        subPos.Height = subRectangle.Height;
        
        scale.X = scaleX;
        scale.Y = scaleY;
        
        if (color == default)
            color = Color.White;

        // Draw!
        batch.Draw(TexturePage.Texture, drawPos, subPos, color * alpha,
            Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), Vector2.Zero, scale, effects, 0);
    }
    
    /// <summary>
    /// Returns a defined attach point for a given frame.
    /// </summary>
    /// <param name="name">The name of the attach point.</param>
    /// <param name="frame">The frame of the sprite that should be searched for the attach point.</param>
    /// <returns>A <see cref="Vector2"/> for the given attach point if it exists;
    /// otherwise an empty <see cref="Vector2"/>.</returns>
    public int[] GetAttachPoint(string name, int frame)
    {
        if (!attachPoints.ContainsKey(name))
            return new[] { 0, 0 };
        
        // TODO: Account for sprite rotation? This needs to be done for rigging support.
        
        var point = (int[])attachPoints[name][Math.Min(frame, attachPoints[name].Length - 1)].Clone();

        point[0] -= (int)Origin.X;
        point[1] -= (int)Origin.Y;

        return point;
    }

    /// <summary>
    /// Generates a precise collision mask based on a single frame of this <see cref="Sprite"/>.
    /// </summary>
    /// <param name="frame">The frame to generate the precise collision mask from.</param>
    /// <returns>A 2D bool array representing a precise collision mask.</returns>
    public bool[,] ToPreciseMask(int frame)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        
        // Copy data into array of our desired length.
        var colorArray = new Color[Width * Height];
        TexturePage.Texture.GetData(0, 0, positions[frame], colorArray, 0, Width * Height);
            
        var outputArray = new bool[Width, Height];
            
        // Split 1D array into a 2D array based on alpha value.
        var pos = 0;
        for (var i = 0; i < Height; i++)
        {
            for (var j = 0; j < Width; j++)
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