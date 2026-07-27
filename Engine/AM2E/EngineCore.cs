using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AM2E.Actors;
using AM2E.Input;
using AM2E.Graphics;
using AM2E.IO;
using AM2E.Networking;
using Microsoft.Xna.Framework.Input;

namespace AM2E;

public sealed class EngineCore : Game
{
    private Action entryPointCallback;
    public static readonly string Version = "2.34.24";
    public static GraphicsDeviceManager _graphics;
    private double updateAccumulator = 0d;
    private const double FRAME_ERROR_MARGIN = .0002;
    private bool resetDeltaTime = false;
    private static EngineCore staticThis;
    public static GameWindow GameWindow;
    internal static string ContentNamespaceHeader;
    internal static string ContentNamespaceFooter;
    internal static string LocalStorageName;
    public static bool DoDebugRender = false;
    public static int DefaultImageEmbedVerticalOffset = 0;

    private static int gameSpeed = 60;
    public static int GameSpeed
    {
        get => gameSpeed;
        set => gameSpeed = Math.Max(1, value);
    }

    public static bool WindowFocused => staticThis.IsActive;

    public const bool DEBUG = true;
    public static bool ImGuiActive = false;
    private ImGuiRenderer imGuiRenderer;
    private AM2EConfig config;

    public EngineCore(string contentNamespaceHeader, string contentNamespaceFooter, AM2EConfig config, Action entryPointCallback)
    {
        AppDomain.CurrentDomain.UnhandledException += Logger.WriteException;

        this.config = config;
        
        ContentNamespaceHeader = contentNamespaceHeader;
        ContentNamespaceFooter = contentNamespaceFooter;
        this.entryPointCallback = entryPointCallback;
        staticThis = this;
        GameWindow = Window;
        LocalStorageName = config.LocalStorageName;
        DefaultImageEmbedVerticalOffset = config.DefaultImageEmbedVerticalOffset;
        
        SetTitle("Built in Another Mediocre 2D Engine");
        
        _graphics = new GraphicsDeviceManager(this)
        {
            GraphicsProfile = config.GraphicsProfile,
            SynchronizeWithVerticalRetrace = config.UseVSync,
            PreferMultiSampling = config.PreferMultiSampling,
        };
        
        Window.AllowUserResizing = config.AllowResizing;
        
        IsMouseVisible = config.IsMouseVisible;
        
        // Timestep fixing 
        InactiveSleepTime = new TimeSpan(0);
        IsFixedTimeStep = false;
        
        InputManager.Initialize(config.InputEnum);
        
        RichTextConfiguration.ApplyConfiguration();
    }

    protected override void Initialize()
    {
        imGuiRenderer = new ImGuiRenderer(this);
        imGuiRenderer.RebuildFontAtlas();
        
        base.Initialize();
        
        Renderer.Initialize(_graphics);
        Renderer.PopulateConfiguration(config);
        
        Window.ClientSizeChanged += Renderer.OnResize;
        
        SetWindowSize(config.DefaultResolutionWidth, config.DefaultResolutionHeight);
        
        _graphics.ApplyChanges();
        
        Logger.Init();

        //ShaderManager.Load();
        //Audio.Load();
        LocalStorage.Initialize();
        
        TextInputEXT.StartTextInput();
        
        // Run supplied entrypoint callback.
        entryPointCallback();
    }

    protected override void LoadContent()
    {
        // Do nothing, content is loaded by other systems as needed via explicit calls to manager classes.
    }

    protected override void Update(GameTime gameTime)
    {
        var printDeltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        var oneOneTwentieth = 1.0 / (GameSpeed * 2);
        var oneSixtieth = 1.0 / GameSpeed;
        var oneThirtieth = 1.0 / (GameSpeed / 2f);

        // https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
        var deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        if (resetDeltaTime)
        {
            deltaTime = oneSixtieth;
            updateAccumulator = 0;
            resetDeltaTime = false;
        }

        if (Math.Abs(deltaTime - oneOneTwentieth) < FRAME_ERROR_MARGIN)
        {
            deltaTime = oneOneTwentieth;
        }

        if (Math.Abs(deltaTime - oneSixtieth) < FRAME_ERROR_MARGIN)
        {
            deltaTime = oneSixtieth;
        }

        if (Math.Abs(deltaTime - oneThirtieth) < FRAME_ERROR_MARGIN)
        {
            deltaTime = oneThirtieth;
        }

        updateAccumulator += deltaTime;
        updateAccumulator = Math.Clamp(updateAccumulator, 0.0, 8.0 / GameSpeed);

        while (updateAccumulator >= oneSixtieth)
        {
            NetworkManager.NetworkTick();
            
            InputManager.Update();
            ActorManager.UpdateActors();
            
            NetworkManager.NetworkFlush();
            
            updateAccumulator -= oneSixtieth;
        }

        Audio.Update();

        base.Update(gameTime);
        
        Logger.DispatchWrite();
        Logger.UpdateCache();
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.Render();

        if (!ImGuiActive)
            return;

        imGuiRenderer.BeforeLayout(gameTime);
        OnImGuiRender();
        imGuiRenderer.AfterLayout();
    }

    public static event Action OnImGuiRender = () =>
    {
        
    };

    // Call after doing heavy loading routines to prevent attempts to catch up on missed frames.
    public static void ResetDeltaTime()
    {
        staticThis.resetDeltaTime = true;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    public static void SetTitle(string title)
    {
        staticThis.Window.Title = title;
    }
    
    /// <summary>
    /// Sets the window size.
    /// </summary>
    /// <param name="width">The desired window width, in pixels.</param>
    /// <param name="height">The desired window height, in pixels.</param>
    public static void SetWindowSize(int width, int height)
    {
        // Disable OnResize event.
        GameWindow.ClientSizeChanged -= Renderer.OnResize;
        
        // Set preferred size in the GDM and then apply the changes.
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
        
        // Re-enable OnResize event.
        GameWindow.ClientSizeChanged += Renderer.OnResize;

        // Run the OnResize event manually to update the draw space and scale.
        Renderer.OnResizeInternal(GameWindow, true);
    }

    
    public static void SetFullscreenScale(int scale)
    {
        Renderer.FullscreenScale = Math.Max(1, scale);
        if (GetFullscreen())
            Renderer.OnResizeInternal(GameWindow, true);
    }

    public static void SetVsync(bool status)
    {
        _graphics.SynchronizeWithVerticalRetrace = status;
        _graphics.ApplyChanges();
    }

    public static bool GetFullscreen()
    {
        return _graphics.IsFullScreen;
    }
    
    public static void SetFullscreen(bool status, int width = 0, int height = 0)
    {
        if (_graphics.IsFullScreen == status)
            return;
        
        // Disable OnResize event.
        GameWindow.ClientSizeChanged -= Renderer.OnResize;
        //_graphics.HardwareModeSwitch = false;
        
        // Set backbuffer size.
        if (status)
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }
        else
        {
            _graphics.PreferredBackBufferWidth =
                width > 0 ? width : GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight =
                height > 0 ? height : GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        _graphics.IsFullScreen = status;
        _graphics.ApplyChanges();
        
        // Re-enable OnResize event.
        GameWindow.ClientSizeChanged += Renderer.OnResize;

        // Run the OnResize event manually to update the draw space and scale.
        Renderer.OnResizeInternal(GameWindow, true);
    }
    
    public static bool GetBorderless()
    {
        return staticThis.Window.IsBorderlessEXT;
    }

    public static void SetBorderless(bool status)
    {
        _graphics.ApplyChanges();
        
        staticThis.Window.IsBorderlessEXT = status;
    }

    public static bool GetMouseVisible()
    {
        return staticThis.IsMouseVisible;
    }

    public static void SetMouseVisible(bool status)
    {
        staticThis.IsMouseVisible = status;
    }

    public static void GracefulExit()
    {
        Logger.Info("Exiting game gracefully. Thank you for using Another Mediocre 2D Engine!");
        Logger.DispatchWrite();
        
        TextInputEXT.StopTextInput();
        
        staticThis.Exit();
    }
}

