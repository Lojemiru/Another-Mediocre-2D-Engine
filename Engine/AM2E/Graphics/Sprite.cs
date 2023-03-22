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
    /// Static <see cref="Vector2"/> used to translate int/int positions for <see cref="SpriteBatch"/> draw calls.
    /// </summary>
    private static Vector2 drawPos;
    
    /// <summary>
    /// A collection of origin transforms for each variant of the <see cref="SpriteEffects"/> passed into draw calls.
    /// </summary>
    private readonly Vector2[] flippedOrigins;
    
    /// <summary>
    /// A collection of locations on this <see cref="Sprite"/>'s <see cref="TexturePage"/>, one entry for each frame.
    /// </summary>
    private readonly Rectangle[] positions;
    
    /// <summary>
    /// Static <see cref="Rectangle"/> used to translate sub-rectangles for sub-rectangle draw calls.
    /// </summary>
    private static Rectangle subPos;
    
    #endregion
    
    
    [JsonConstructor]
    public Sprite([JsonProperty("length")] int length, [JsonProperty("originX")] int originX,
        [JsonProperty("originY")] int originY, [JsonProperty("attachPoints")] Dictionary<string, int[][]> attachPoints,
        [JsonProperty("positions")] Rectangle[] positions, [JsonProperty("width")] int width,
        [JsonProperty("height")] int height)
    {
        Length = length;
        Origin = new Vector2(originX, originY);
        this.attachPoints = attachPoints;
        this.positions = positions;
        Width = width;
        Height = height;
        
        // Set up array of origins to match sprite flips at render.
        var flippedOriginX = Width - 1 - Origin.X;
        var flippedOriginY = Height - 1 - Origin.Y;
        flippedOrigins = new[] { Origin, new(flippedOriginX, Origin.Y), new(Origin.X, flippedOriginY), new(flippedOriginX, flippedOriginY) };
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
    public void Draw(SpriteBatch batch, int x, int y, int frame, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1)
    {
        drawPos.X = x;
        drawPos.Y = y;
        batch.Draw(TexturePage.Texture, drawPos, positions[MathHelper.Wrap(frame, 0, Length)], Color.White * alpha,
            Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), flippedOrigins[(int)effects], 1, effects, 0);
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
    public void Draw(SpriteBatch batch, int x, int y, int frame, Rectangle subRectangle, float rotation = 0,
        SpriteEffects effects = SpriteEffects.None, float alpha = 1)
    {
        // Constrain frame to safe indices.
        frame = MathHelper.Wrap(frame, 0, Length);
        drawPos.X = x;
        drawPos.Y = y;
        subPos.X = positions[frame].X + subRectangle.X;
        subPos.Y = positions[frame].Y + subRectangle.Y;
        subPos.Width = subRectangle.Width;
        subPos.Height = subRectangle.Height;
        batch.Draw(TexturePage.Texture, drawPos, subPos, Color.White * alpha,
            Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), Origin, 1, effects, 0);
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
}