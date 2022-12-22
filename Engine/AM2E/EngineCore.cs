using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AM2E.Actors;
using AM2E.Control;
using System;
using System.Diagnostics;
using AM2E.Graphics;

namespace AM2E;

public class EngineCore : Game
{
    public static GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private double updateAccumulator = 0d;
    private const double FRAME_ERROR_MARGIN = .0002;
    private const double MAX_ACCUMULATOR_VALUE = 8.0 / 60.0;
    private bool resetDeltaTime = false;

    public EngineCore()
    {
        // TODO: Load parameters from config class/object

        _graphics = new GraphicsDeviceManager(this);
        Window.ClientSizeChanged += Renderer.OnResize;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        // Timestep fixing 
        InactiveSleepTime = new TimeSpan(0);
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = true;
    }

    protected override void Initialize()
    {
        BadCamera.Initialize(_graphics);

        Renderer.Initialize(_graphics);

        // TODO: Not a huge fan of forcing a layer here. Find a better entry point for the game content...
        Renderer.AddLayer("Control", -999999);
        ActorManager.Instantiate(new GameContent.GameManager(0, 0), "Control");

        // TODO: Proper initialization logic.

        Window.Title = "Another Mediocre 2D Engine";
        Window.AllowUserResizing = true;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        double printDeltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        float fps = 1 / (float)printDeltaTime;
        //Debug.WriteLine(fps);

        var GAME_SPEED = 60;

        // TODO: Change these to ints instead of doubles. Could involve a messy conversion on TotalSeconds...
        // https://medium.com/@tglaiel/how-to-make-your-game-run-at-60fps-24c61210fe75
        var deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        var oneOneTwentieth = 1.0 / GAME_SPEED * 2;
        var oneSixtieth = 1.0 / GAME_SPEED;
        var oneThirtieth = 1.0 / GAME_SPEED / 2;

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
            FixedUpdate();
            updateAccumulator -= oneSixtieth;
        }

        base.Update(gameTime);
    }

    private void FixedUpdate()
    {
        InputManager.Update();
        ActorManager.UpdateActors();
        BadCamera.Step();
    }

    protected override void Draw(GameTime gameTime)
    {
        Renderer.Render();
    }

    // Call after doing heavy loading routines to prevent attempts to catch up on missed frames.
    public void ResetDeltaTime()
    {
        resetDeltaTime = true;
    }
}