using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using AM2E.Graphics;
using Microsoft.Xna.Framework;

namespace AM2E.Control;

// TODO: Review EngineConfig setup. Is this a good design paradigm?

public static class InputManager
{
    private static readonly Dictionary<Input, KeyboardInput> KeyboardListeners = new();
    private static readonly Dictionary<Input, MouseInput> MouseListeners = new();
    private static readonly Dictionary<Input, GamePadInput> GamePadListeners = new();

    public static float RightCenterDeadzone = 0.1f;
    public static float LeftCenterDeadzone = 0.1f;
    public static GamePadDeadZone CenterDeadZoneType = GamePadDeadZone.Circular;
    public static float DiagonalDeadZone = 15f;

    // TODO: Make this auto-swap?
    public static readonly int GamePadIndex = 0;

    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }

    static InputManager()
    {
        foreach (Input input in Enum.GetValues(typeof(Input)))
        {
            KeyboardListeners.Add(input, new KeyboardInput(Keys.None));
            MouseListeners.Add(input, new MouseInput(MouseButton.None));
            GamePadListeners.Add(input, new GamePadInput(Buttons.None));
        }
    }

    public static void Update()
    {
        var keyboardState = Keyboard.GetState();
        foreach (var listener in KeyboardListeners.Values)
        {
            listener.Poll(keyboardState);
        }
        
        var mouseState = Mouse.GetState();
        foreach (var listener in MouseListeners.Values)
        {
            listener.Poll(mouseState);
        }

        MouseX = Math.Clamp(Camera.BoundLeft + (int)((mouseState.X - Renderer.ApplicationSpace.X) * ((float)Renderer.GameWidth / Renderer.ApplicationSpace.Width)), Camera.BoundLeft, Camera.BoundRight);
        MouseY = Math.Clamp(Camera.BoundTop + (int)((mouseState.Y - Renderer.ApplicationSpace.Y) * ((float)Renderer.GameHeight / Renderer.ApplicationSpace.Height)), Camera.BoundTop, Camera.BoundBottom);
        
        var gamePadState = GamePad.GetState(GamePadIndex);
        foreach (var listener in GamePadListeners.Values)
        {
            listener.Poll(gamePadState);
        }
    }

    // TODO: Rebinding. Needs to handle smart swapping via groups.

    // TODO: Alternate bindings. Should probably be implemented via InputBase but will need support here.

    // TODO: Cancelling input checkers.

    public static void BindKey(Input input, Keys key)
    {
        KeyboardListeners[input].Rebind(key);
    }

    public static void BindMouseButton(Input input, MouseButton mouseButton)
    {
        MouseListeners[input].Rebind(mouseButton);
    }

    public static void BindGamePadButton(Input input, Buttons button)
    {
        GamePadListeners[input].Rebind(button);
    }

    public static void Remove(Input input)
    {
        KeyboardListeners.Remove(input);
        MouseListeners.Remove(input);
        GamePadListeners.Remove(input);
    }

    #region Getters

    public static bool GetPressed(Input input)
    {
        return KeyboardListeners[input].InputPressed | MouseListeners[input].InputPressed | GamePadListeners[input].InputPressed;
    }

    public static bool GetReleased(Input input)
    {
        return KeyboardListeners[input].InputReleased | MouseListeners[input].InputReleased | GamePadListeners[input].InputReleased;
    }

    public static bool GetHeld(Input input)
    {
        return KeyboardListeners[input].InputHeld | MouseListeners[input].InputHeld | GamePadListeners[input].InputHeld;
    }

    public static int GetHeldSteps(Input input)
    {
        return KeyboardListeners[input].InputHeldSteps | MouseListeners[input].InputHeldSteps | GamePadListeners[input].InputHeldSteps;
    }

    #endregion
}