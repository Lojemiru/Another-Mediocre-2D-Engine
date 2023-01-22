﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using AM2E.Levels;
using LanguageExt;

namespace AM2E.Graphics
{
    public static class Renderer
    {
        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        private static Rectangle applicationSpace = new Rectangle(0, 0, 426, 240);
        private static SpriteBatch applicationBatch;

        public static int UpscaleAmount { get; private set; } = 1;

        // TODO: This should probably be configured/controlled elsewhere.
        public static float TargetRatio = 16 / (float)9;
        
        // TODO: Pull from engine config instead :)
        // TODO: Figure out accessor proper
        public static RenderTarget2D ApplicationSurface;

        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            Renderer.GraphicsDeviceManager = graphicsDeviceManager;
            // TODO: Pull size values from config :)
            SetGameResolution(1920, 1080);
            applicationBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        }

        public static void SetGameResolution(int width, int height)
        {
            ApplicationSurface = new RenderTarget2D(GraphicsDeviceManager.GraphicsDevice, width * UpscaleAmount, height * UpscaleAmount, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
            GameCamera.UpdateTransform();
        }

        public static void SetUpscaleAmount(int amount)
        {
            UpscaleAmount = amount;
            SetGameResolution(ApplicationSurface.Width, ApplicationSurface.Height);
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
        }
        
        public static void Render()
        {
            // Target and clear application surface.
            GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(ApplicationSurface);
            GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            
            // Draw each layer.
            // TODO: Order these when drawing lol
            // TODO: Depth is insanely scuffed here, somehow. I don't understand sprite batches YAAAAAAAY
            World.RenderLevels();
            
            // Reset render target, clear backbuffer.
            GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
            GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            
            // Render application surface into drawable application space.
            applicationBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            applicationBatch.Draw(ApplicationSurface, applicationSpace, Color.White);
            applicationBatch.End();
        }

        public static void SetRenderTarget(RenderTarget2D target)
        {
            GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(target);
        }
    }
}
