using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using LanguageExt;

namespace AM2E.Graphics
{
    public static class Renderer
    {
        private static Dictionary<string, Layer> layers = new();
        private static GraphicsDeviceManager graphicsDeviceManager;
        private static Rectangle applicationSpace = new Rectangle(0, 0, 426, 240);
        private static SpriteBatch applicationBatch;

        // TODO: This should probably be configured/controlled elsewhere.
        public static float TargetRatio = 16 / (float)9;
        
        // TODO: Pull from engine config instead :)
        // TODO: Figure out accessor proper
        public static RenderTarget2D ApplicationSurface;

        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            Renderer.graphicsDeviceManager = graphicsDeviceManager;
            // TODO: Pull size values from config :)
            ApplicationSurface = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, 426, 240, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
            applicationBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
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

        public static void OnResize(Object sender, EventArgs e)
        {
            var window = (GameWindow)sender;
            
            // Thanks be to http://www.infinitespace-studios.co.uk/general/monogame-scaling-your-game-using-rendertargets-and-touchpanel/
            
            
            float outputAspect = window.ClientBounds.Width / (float)window.ClientBounds.Height;

            if (outputAspect <= TargetRatio)
            {
                // output is taller than it is wider, bars on top/bottom
                int presentHeight = (int)((window.ClientBounds.Width / TargetRatio) + 0.5f);
                int barHeight = (window.ClientBounds.Height - presentHeight) / 2;
                applicationSpace = new Rectangle(0, barHeight, window.ClientBounds.Width, presentHeight);
            }
            else
            {
                // output is wider than it is tall, bars left/right
                int presentWidth = (int)((window.ClientBounds.Height * TargetRatio) + 0.5f);
                int barWidth = (window.ClientBounds.Width - presentWidth) / 2;
                applicationSpace = new Rectangle(barWidth, 0, presentWidth, window.ClientBounds.Height);
            }
            
            
            
            Debug.WriteLine(window.ClientBounds.Size);
        }
        
        public static void Render()
        {
            graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            graphicsDeviceManager.GraphicsDevice.SetRenderTarget(ApplicationSurface);
            
            // TODO: Order these when drawing lol
            foreach (Layer layer in layers.Values)
            {
                layer.Draw();
            }
            
            graphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
            
            graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            
            applicationBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, transformMatrix:BadCamera.Matrix);
            applicationBatch.Draw(ApplicationSurface, applicationSpace, Color.White);
            applicationBatch.End();
            
        }
    }
}
