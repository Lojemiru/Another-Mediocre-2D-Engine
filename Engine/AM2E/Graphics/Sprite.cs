using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GameContent;

namespace AM2E.Graphics
{
    public class Sprite
    {
        public int Length { get; }
        public Vector2 Origin { get; }
        
        private readonly Dictionary<string, int[][]> attachPoints;

        // TODO: This needs to return something read-only. As it stands the attach point could be modified on the other side...
        public int[] GetAttachPoint(string name, int frame)
        {
            if (!attachPoints.ContainsKey(name))
                return new[] { 0, 0 };

            var point = (int[])attachPoints[name][Math.Min(frame, attachPoints[name].Length - 1)].Clone();

            point[0] -= (int)Origin.X;
            point[1] -= (int)Origin.Y;

            return point;
        }

        private readonly Rectangle[] positions;

        public int Width { get; }
        public int Height { get; }
        public TexturePage TexturePage { get; set; }

        private readonly Vector2[] origins;

        [JsonConstructor]
        public Sprite([JsonProperty("length")] int length, [JsonProperty("originX")] int originX, [JsonProperty("originY")] int originY, [JsonProperty("attachPoints")] Dictionary<string, int[][]> attachPoints, [JsonProperty("positions")] Rectangle[] positions, [JsonProperty("width")] int width, [JsonProperty("height")] int height)
        {
            Length = length;
            Origin = new Vector2(originX, originY);
            this.attachPoints = attachPoints;
            this.positions = positions;
            Width = width;
            Height = height;
            // TODO: Currently handles flipping independently. Should probably handle doing both?
            origins = new[] { Origin, new(Width - 1 - Origin.X, Origin.Y), new(Origin.X, Height - 1 - Origin.Y) };
        }

        public void Draw(SpriteBatch batch, int x, int y, int frame, float rotation = 0, SpriteEffects effects = SpriteEffects.None, float alpha = 1)
        {
            // TODO: Make frame safe!
            Vector2 pos = new(x, y);
            // TODO: Review that depth setting here doesn't break anything.
            batch.Draw(TexturePage.Texture, pos, positions[frame], Color.White * alpha, Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), origins[(int)effects], 1, effects, 0);
        }

        public void Draw(SpriteBatch batch, int x, int y, int frame, Rectangle subRectangle, float rotation = 0,
            SpriteEffects effects = SpriteEffects.None, float alpha = 1)
        {
            // TODO: Make frame safe!
            Vector2 pos = new(x, y);
            // TODO: Review that depth setting here doesn't break anything.
            Rectangle subPos = new(positions[frame].X + subRectangle.X, positions[frame].Y + subRectangle.Y, subRectangle.Width, subRectangle.Height);
            batch.Draw(TexturePage.Texture, pos, subPos, Color.White * alpha, Microsoft.Xna.Framework.MathHelper.ToRadians(rotation), Origin, 1, effects, 0);
        }
        
        
        public bool[,] ToPreciseMask(int frame)
        {
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
    }
}
