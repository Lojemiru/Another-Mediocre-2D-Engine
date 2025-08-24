using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AM2E.Levels;

namespace AM2E.Graphics;

// TODO: Run entire canvas through https://github.com/libretro/glsl-shaders/blob/master/interpolation/shaders/ControlledSharpness.glsl ?

public static class Renderer
{
    public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
    public static Rectangle ApplicationSpace;
    private static Rectangle guiSpace = new();
    private static SpriteBatch applicationBatch;
    internal static SpriteBatch GuiBatch;
    private static Vector2 posVector = new(0, 0);
    public static int GameWidth;
    public static int GameHeight;
    public static float OffsetX = 0;
    public static float OffsetY = 0;
    private static Rectangle finalApplicationSpace = new();

    internal static int UpscaleAmount { get; private set; } = 1;

    private static float targetRatio;
    
    public static RenderTarget2D ApplicationSurface { get; private set; }
    
    private static RenderTarget2D guiSurface;

    internal static void PopulateConfiguration(AM2EConfig config)
    {
        targetRatio = config.TargetAspectRatio;
        SetGameResolution(config.DefaultResolutionWidth, config.DefaultResolutionHeight);
    }

    public static void SetTargetRatio(float ratio)
    {
        targetRatio = ratio;
        OnResizeInternal(EngineCore.StaticWindow);
    }

    public static event Action<SpriteBatch> OnDebugRender = _ => { };

    public static event Action<SpriteBatch> OnGUIRender = _ => { };
    
    public static void DebugRender()
    {
        applicationBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
        OnDebugRender(applicationBatch);
        applicationBatch.End();
    }

    internal static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
    {
        GraphicsDeviceManager = graphicsDeviceManager;
        applicationBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        GuiBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
    }

    public static void SetGameResolution(int width, int height)
    {
        GameWidth = width;
        GameHeight = height;
        ApplicationSurface = new RenderTarget2D(GraphicsDeviceManager.GraphicsDevice, width * UpscaleAmount,
            height * UpscaleAmount, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
        
        guiSurface = new RenderTarget2D(GraphicsDeviceManager.GraphicsDevice, width, height);

        Camera.UpdateTransform();
        
        OnResizeInternal(EngineCore.StaticWindow);
    }

    public static void SetUpscaleAmount(int amount)
    {
        UpscaleAmount = amount;
        SetGameResolution(ApplicationSurface.Width, ApplicationSurface.Height);
    }

    internal static void OnResize(object sender, EventArgs e)
        => OnResizeInternal(sender as GameWindow);
    

    internal static void OnResizeInternal(GameWindow window, bool fromManualResize = false)
    {
        if (!fromManualResize)
        {
            window.ClientSizeChanged -= OnResize;

            GraphicsDeviceManager.PreferredBackBufferWidth = window.ClientBounds.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = window.ClientBounds.Height;
            GraphicsDeviceManager.ApplyChanges();

            window.ClientSizeChanged += OnResize;
        }

        // Thanks be to http://www.infinitespace-studios.co.uk/general/monogame-scaling-your-game-using-rendertargets-and-touchpanel/

        var outputAspect = window.ClientBounds.Width / (float)window.ClientBounds.Height;

        if (outputAspect <= targetRatio)
        {
            // output is taller than it is wider, bars on top/bottom
            var presentHeight = (int)((window.ClientBounds.Width / targetRatio) + 0.5f);
            var barHeight = (window.ClientBounds.Height - presentHeight) / 2;
            ApplicationSpace = new Rectangle(0, barHeight, window.ClientBounds.Width, presentHeight);
        }
        else
        {
            // output is wider than it is tall, bars left/right
            var presentWidth = (int)((window.ClientBounds.Height * targetRatio) + 0.5f);
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
        
        if (EngineCore.DoDebugRender)
            DebugRender();
        
        // Target and clear GUI surface.
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(guiSurface);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);

        // Render GUI surface.
        GuiBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        OnGUIRender(GuiBatch);
        GuiBatch.End();
        
        // Reset render target, clear backbuffer.
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);

        var texelX = ApplicationSpace.Width / GameWidth;
        var texelY = ApplicationSpace.Height / GameHeight;

        finalApplicationSpace = new Rectangle(ApplicationSpace.X + (int)Math.Floor(OffsetX * texelX), ApplicationSpace.Y + (int)Math.Floor(OffsetY * texelY), ApplicationSpace.Width, ApplicationSpace.Height);
        
        // Render application and GUI surfaces into drawable application space.
        applicationBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        applicationBatch.Draw(ApplicationSurface, finalApplicationSpace, Color.White);
        applicationBatch.Draw(guiSurface, guiSpace, Color.White);
        applicationBatch.End();
    }

    public static void SetRenderTarget(RenderTarget2D target)
    {
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(target);
    }

    public static void CopyApplicationSurfaceTo(SpriteBatch spriteBatch, RenderTarget2D target)
    {
        spriteBatch.End();
        
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(target);
        GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);
        
        spriteBatch.Begin();
        spriteBatch.Draw(ApplicationSurface, posVector, Color.White);
        spriteBatch.End();
        
        GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(ApplicationSurface);
        spriteBatch.Begin();
        spriteBatch.Draw(target, posVector, Color.White);
    }
}