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

    internal static GameWindow Window;
    
    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }

    static InputManager()
    {
        foreach (Input input in Enum.GetValues(typeof(Input)))
        {
            KeyboardListeners.Add(input, new KeyboardInput(Keys.None));
            MouseListeners.Add(input, new MouseInput(MouseButton.None));
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

        MouseX = Math.Clamp(Camera.BoundLeft + (int)((mouseState.X - Renderer.ApplicationSpace.X) / ((float)Renderer.ApplicationSpace.Width / Renderer.GameWidth)), Camera.BoundLeft, Camera.BoundRight);
        MouseY = Math.Clamp(Camera.BoundTop + (int)((mouseState.Y - Renderer.ApplicationSpace.Y) / ((float)Renderer.ApplicationSpace.Height / Renderer.GameHeight)), Camera.BoundTop, Camera.BoundBottom);

        foreach (var listener in MouseListeners.Values)
        {
            listener.Poll(mouseState);
        }
    }

    // TODO: Rebinding. Needs to handle smart swapping via groups.

    // TODO: Alternate bindings. Should probably be implemented via KeyboardInput but will need support here.

    // TODO: Controller input.

    // TODO: Cancelling input checkers.
    
    // TODO: Mouse input

    public static void BindKey(Input input, Keys key)
    {
        KeyboardListeners[input].Rebind(key);
    }

    public static void BindMouseButton(Input input, MouseButton mouseButton)
    {
        MouseListeners[input].Rebind(mouseButton);
    }

    public static void Remove(Input input)
    {
        KeyboardListeners.Remove(input);
        MouseListeners.Remove(input);
    }

    #region Getters

    public static bool GetPressed(Input input)
    {
        return KeyboardListeners[input].InputPressed | MouseListeners[input].InputPressed;
    }

    public static bool GetReleased(Input input)
    {
        return KeyboardListeners[input].InputReleased | MouseListeners[input].InputReleased;
    }

    public static bool GetHeld(Input input)
    {
        return KeyboardListeners[input].InputHeld | MouseListeners[input].InputHeld;
    }

    public static int GetHeldSteps(Input input)
    {
        return KeyboardListeners[input].InputHeldSteps | MouseListeners[input].InputHeldSteps;
    }

    #endregion
}