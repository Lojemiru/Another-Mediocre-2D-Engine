using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using AM2E.Graphics;

namespace AM2E.Control;

// TODO: Review EngineConfig setup. Is this a good design paradigm?

public static class InputManager
{
    private static readonly Dictionary<Input, KeyboardInput> KeyboardListeners = new();
    private static readonly Dictionary<Input, MouseInput> MouseListeners = new();
    private static readonly Dictionary<Input, GamePadInput> GamePadListeners = new();

    public static float RightCenterDeadZone = 0.1f;
    public static float LeftCenterDeadZone = 0.1f;
    public static GamePadDeadZone CenterDeadZoneType = GamePadDeadZone.Circular;
    public static float AngularAxisDeadZone = 15f;

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
            listener.Update(keyboardState);
        }
        
        var mouseState = Mouse.GetState();
        foreach (var listener in MouseListeners.Values)
        {
            listener.Update(mouseState);
        }

        MouseX = Math.Clamp(Camera.BoundLeft + (int)((mouseState.X - Renderer.ApplicationSpace.X) * ((float)Renderer.GameWidth / Renderer.ApplicationSpace.Width)), Camera.BoundLeft, Camera.BoundRight);
        MouseY = Math.Clamp(Camera.BoundTop + (int)((mouseState.Y - Renderer.ApplicationSpace.Y) * ((float)Renderer.GameHeight / Renderer.ApplicationSpace.Height)), Camera.BoundTop, Camera.BoundBottom);
        
        var gamePadState = GamePad.GetState(GamePadIndex);
        foreach (var listener in GamePadListeners.Values)
        {
            listener.Update(gamePadState);
        }
    }

    // TODO: Rebinding. Needs to handle smart swapping via groups.

    public static void BindKey(Input input, Keys key, int index = 0)
    {
        KeyboardListeners[input].Rebind(key, index);
    }

    public static void BindMouseButton(Input input, MouseButton mouseButton, int index = 0)
    {
        MouseListeners[input].Rebind(mouseButton, index);
    }

    public static void BindGamePadButton(Input input, Buttons button, int index = 0)
    {
        GamePadListeners[input].Rebind(button, index);
    }
    
    public static int BindAlternateKey(Input input, Keys key)
    {
        return KeyboardListeners[input].AddAlternateBinding(key);
    }

    public static int BindAlternateMouseButton(Input input, MouseButton mouseButton)
    {
        return MouseListeners[input].AddAlternateBinding(mouseButton);
    }

    public static int BindAlternateGamePadButton(Input input, Buttons button)
    {
        return GamePadListeners[input].AddAlternateBinding(button);
    }

    #region Getters

    public static bool GetPressed(Input input)
    {
        return KeyboardListeners[input].InputPressed | MouseListeners[input].InputPressed | GamePadListeners[input].InputPressed;
    }

    public static bool GetPressedCancelling(Input input, Input cancellingInput)
    {
        return GetPressed(input) && !GetHeld(cancellingInput);
    }

    public static bool GetReleased(Input input)
    {
        return KeyboardListeners[input].InputReleased | MouseListeners[input].InputReleased | GamePadListeners[input].InputReleased;
    }

    public static bool GetReleasedCancelling(Input input, Input cancellingInput)
    {
        return GetReleased(input) && !GetHeld(cancellingInput);
    }

    public static bool GetHeld(Input input)
    {
        return KeyboardListeners[input].InputHeld | MouseListeners[input].InputHeld | GamePadListeners[input].InputHeld;
    }

    public static bool GetHeldCancelling(Input input, Input cancellingInput)
    {
        return GetHeld(input) && !GetHeld(cancellingInput);
    }

    public static int GetHeldSteps(Input input)
    {
        return KeyboardListeners[input].InputHeldSteps | MouseListeners[input].InputHeldSteps | GamePadListeners[input].InputHeldSteps;
    }

    #endregion
}