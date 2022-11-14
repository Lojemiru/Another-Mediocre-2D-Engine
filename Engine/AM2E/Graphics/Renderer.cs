using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Graphics
{
    public static class Renderer
    {
        private static Dictionary<string, Layer> layers = new();
        private static GraphicsDeviceManager graphicsDevice;

        public static void Initialize(GraphicsDeviceManager graphicsDevice)
        {
            Renderer.graphicsDevice = graphicsDevice;
        }

        public static void AddLayer(string name, int depth)
        {
            layers.Add(name, new Layer(name, depth));
        }

        public static Layer GetLayer(string name)
        {
            return layers[name];
        }

        public static void AddDrawable(string layer, IDrawable drawable)
        {
            layers[layer].Add(drawable);
        }

        public static void AddDrawable(Layer layer, IDrawable drawable)
        {
            AddDrawable(layer.Name, drawable);
        }

        public static void RemoveDrawable(string layer, IDrawable drawable)
        {
            layers[layer].Remove(drawable);
        }

        public static void RemoveDrawable(Layer layer, IDrawable drawable)
        {
            RemoveDrawable(layer.Name, drawable);
        }

        public static void Render()
        {
            // TODO: Order these when drawing lol
            foreach (Layer layer in layers.Values)
            {
                layer.Draw();
            }
        }
    }
}
