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
    internal static Server Server;
    internal static Client Client;
    public static bool isNetworked = false;
    public static bool isServer;


    public const bool DEBUG = true;

    public EngineCore(Action entryPointCallback)
    {
        this.entryPointCallback = entryPointCallback;
        staticThis = this;
        
        SetTitle("Another Mediocre 2D Engine");
        Window.AllowUserResizing = true;

        // TODO: Load parameters from config class/object

        _graphics = new GraphicsDeviceManager(this);
        Window.ClientSizeChanged += Renderer.OnResize;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        // Timestep fixing 
        InactiveSleepTime = new TimeSpan(0);
        IsFixedTimeStep = false;
        
        // TODO: Everything here but vsync is boilerplate from M3D. Review!
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.SynchronizeWithVerticalRetrace = true;
        _graphics.PreferMultiSampling = false;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Renderer.Initialize(_graphics);
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

