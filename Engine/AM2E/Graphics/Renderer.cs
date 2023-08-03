using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using AM2E.Levels;

namespace AM2E.Graphics;

public static class Renderer
{
    public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
    public static Rectangle ApplicationSpace = new Rectangle(0, 0, 426, 240);
    private static Rectangle guiSpace = new();
    private static SpriteBatch applicationBatch;
    private static SpriteBatch guiBatch;
    public static int GameWidth;
    public static int GameHeight;

    public static int UpscaleAmount { get; private set; } = 1;

    // TODO: This should probably be configured/controlled elsewhere.
    public static float TargetRatio = 16 / (float)9;
        
    // TODO: Pull from engine config instead :)
    // TODO: Figure out accessor proper
    public static RenderTarget2D ApplicationSurface;
    private static RenderTarget2D guiSurface;

    public static event Action<SpriteBatch> OnDebugRender = _ => { };

    public static event Action<SpriteBatch> OnGUIRender = (_) => { };
    
    public static void DebugRender(SpriteBatch spriteBatch)
    {
        OnDebugRender(spriteBatch);
    }

    internal static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
    {
        GraphicsDeviceManager = graphicsDeviceManager;
        // TODO: Pull size values from config :)
        SetGameResolution(1920, 1080);
        applicationBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        guiBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
    }

    public static void SetGameResolution(int width, int height)
    {
        GameWidth = width;
        GameHeight = height;
        ApplicationSurface = new RenderTarget2D(GraphicsDeviceManager.GraphicsDevice, width * UpscaleAmount,
            height * UpscaleAmount, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
        
        guiSurface = new RenderTarget2D(GraphicsDeviceManager.GraphicsDevice, width, height);

        Camera.UpdateTransform();
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
            
            
        var outputAspect = window.ClientBounds.Width / (float)window.ClientBounds.Height;

        if (outputAspect <= TargetRatio)
        {
            // output is taller than it is wider, bars on top/bottom
            var presentHeight = (int)((window.ClientBounds.Width / TargetRatio) + 0.5f);
            var barHeight = (window.ClientBounds.Height - presentHeight) / 2;
            ApplicationSpace = new Rectangle(0, barHeight, window.ClientBounds.Width, presentHeight);
        }
        else
        {
            // output is wider than it is tall, bars left/right
            var presentWidth = (int)((window.ClientBounds.Height * TargetRatio) + 0.5f);
            var barWidth = (window.ClientBounds.Width - presentWidth) / 2;
            ApplicationSpace = new Rectangle(barWidth, 0, presentWidth, window.ClientBounds.Height);
        }
        
        guiSpace.X = ApplicationSpace.X;
        guiSpace.Y = ApplicationSpace.Y;
        guiSpace.Width = ApplicationSpace.Width;
        guiSpace.Height = ApplicationSpace.Height;
    }

    public static void Render()
    {
        // Target and clear application surface.
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(ApplicationSurface);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            
        // Draw each layer.
        World.RenderLevels();
        
        // Target and clear GUI surface.
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(guiSurface);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

        // Render GUI surface.
        guiBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        OnGUIRender(guiBatch);
        guiBatch.End();
        
        // Reset render target, clear backbuffer.
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);

        // Render application and GUI surfaces into drawable application space.
        applicationBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        applicationBatch.Draw(ApplicationSurface, ApplicationSpace, Color.White);
        applicationBatch.Draw(guiSurface, guiSpace, Color.White);
        applicationBatch.End();
    }

    public static void SetRenderTarget(RenderTarget2D target)
    {
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(target);
    }
}