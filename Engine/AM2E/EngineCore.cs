using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AM2E.Actors;
using AM2E.Control;
using System;
using AM2E.Graphics;
using AM2E.Networking;

namespace AM2E;

public sealed class EngineCore : Game
{
    private Action entryPointCallback;
    public static readonly string Version = "ALPHA";
    public static GraphicsDeviceManager _graphics;
    private double updateAccumulator = 0d;
    private const double FRAME_ERROR_MARGIN = .0002;
    private const double MAX_ACCUMULATOR_VALUE = 8.0 / 60.0;
    private bool resetDeltaTime = false;
    private static EngineCore staticThis;
    internal static GameWindow StaticWindow;
    internal static Server Server;
    internal static Client Client;
    internal static string ContentNamespace;
    public static bool isNetworked = false;
    public static bool isServer;

    public const bool DEBUG = true;

    public EngineCore(string contentNamespace, AM2EConfig config, Action entryPointCallback)
    {
        ContentNamespace = contentNamespace;
        this.entryPointCallback = entryPointCallback;
        staticThis = this;
        StaticWindow = Window;
        
        SetTitle("Built in Another Mediocre 2D Engine");
        
        Window.AllowUserResizing = config.AllowResizing;
        
        _graphics = new GraphicsDeviceManager(this);
        
        Window.ClientSizeChanged += Renderer.OnResize;
        
        IsMouseVisible = config.IsMouseVisible;
        
        // Timestep fixing 
        InactiveSleepTime = new TimeSpan(0);
        IsFixedTimeStep = false;
        
        // TODO: Everything here but vsync is boilerplate from M3D. Review!
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.SynchronizeWithVerticalRetrace = config.UseVSync;
        _graphics.PreferMultiSampling = false;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.ApplyChanges();
        
        Renderer.Initialize(_graphics);
        Renderer.PopulateConfiguration(config);
        
        SetWindowSize(config.DefaultResolutionWidth, config.DefaultResolutionHeight);
    }

    protected override void Initialize()
    {
        Audio.Init();

        // Run supplied entrypoint callback.
        entryPointCallback();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Do nothing, content is loaded by other systems as needed via explicit calls to manager classes.
    }

    protected override void Update(GameTime gameTime)
    {
        var printDeltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        const int GAME_SPEED = 60;
        const double oneOneTwentieth = 1.0 / GAME_SPEED * 2;
        const double oneSixtieth = 1.0 / GAME_SPEED;
        const double oneThirtieth = 1.0 / GAME_SPEED / 2;
        
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
        updateAccumulator = Math.Clamp(updateAccumulator, 0.0, MAX_ACCUMULATOR_VALUE);

        while (updateAccumulator >= oneSixtieth)
        {
            NetworkUpdate();
            FixedUpdate();
            updateAccumulator -= oneSixtieth;
        }
        
        Audio.Update();

        base.Update(gameTime);
    }
    private static void FixedUpdate()
    {
        InputManager.Update();
        ActorManager.UpdateActors(false);
        
        CommandConsole.ExecuteDeferredCommand();
    }

    private static void NetworkUpdate()
    {
        if (!isNetworked)
            return;
        
        if (isServer)
        {
            Server.Update();
        }
        else
        {
            Client.Update();
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        /*
        var s = $"Frametime: {gameTime.ElapsedGameTime}";

        Console.WriteLine(s);

        if (gameTime.ElapsedGameTime.Milliseconds > 16) {
            Console.WriteLine($"{gameTime.TotalGameTime} {gameTime.ElapsedGameTime} Lag!");
        }
        */
        
        Renderer.Render();
    }

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
        StaticWindow.ClientSizeChanged -= Renderer.OnResize;
        
        // Set preferred size in the GDM and then apply the changes.
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
        
        // Re-enable OnResize event.
        StaticWindow.ClientSizeChanged += Renderer.OnResize;

        // Run the OnResize event manually to update the draw space and scale.
        Renderer.OnResizeInternal(StaticWindow, true);
    }

    public static void StartServer(int port)
    {
        Server = new Server(port);
        isNetworked = true;
        isServer = true;
    }

    public static void StartClient(string ip, int port)
    {
        Client = new Client(ip, port);
        isNetworked = true;
        isServer = false;
    }
}

