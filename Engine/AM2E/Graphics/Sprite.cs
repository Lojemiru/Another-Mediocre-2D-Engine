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

namespace AM2E.Graphics
{
    public class Sprite
    {
        public int Length { get; private set; }
        public Vector2 Origin { get; private set; }
        
        private Dictionary<string, int[][]> attachPoints;

        // TODO: This needs to return something read-only. As it stands the attach point could be modified on the other side...
        public int[] GetAttachPoint(string name, int frame)
        {
            if (!attachPoints.ContainsKey(name))
                return new int[] { 0, 0 };

            int[] point = (int[])attachPoints[name][Math.Min(frame, attachPoints[name].Length - 1)].Clone();

            point[0] -= (int)Origin.X;
            point[1] -= (int)Origin.Y;

            return point;
        }

        private Rectangle[] positions;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public TexturePage TexturePage { get; set; }

        private Vector2[] origins;

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
            origins = new Vector2[] { Origin, new Vector2(Width - 1 - Origin.X, Origin.Y), new Vector2(Origin.X, Height - 1 - Origin.Y) };
        }

        public void Draw(SpriteBatch batch, int x, int y, int frame, float rotation = 0, SpriteEffects effects = SpriteEffects.None, float alpha = 1)
        {
            // TODO: Make frame safe!
            Vector2 pos = new(x, y);
            // TODO: Review that depth setting here doesn't break anything.
            batch.Draw(TexturePage.Texture, pos, positions[frame], Color.White * alpha, MathHelper.ToRadians(rotation), origins[(int)effects], 1, effects, 0);
        }
    }
}
