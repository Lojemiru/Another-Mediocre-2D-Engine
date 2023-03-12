using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace AM2E.Control;

// TODO: Review EngineConfig setup. Is this a good design paradigm?

public static class InputManager
{
    private static readonly Dictionary<Input, KeyboardInput> KeyboardListeners = new();
        
    static InputManager()
    {
        foreach (Input input in Enum.GetValues(typeof(Input)))
        {
            KeyboardListeners.Add(input, new KeyboardInput(Keys.None));
        }
    }

    public static void Update()
    {
        var keyboardState = Keyboard.GetState();
        var valColl = KeyboardListeners.Values;
        foreach (var listener in valColl)
        {
            listener.Poll(keyboardState);
        }
    }

    // TODO: Rebinding. Needs to handle smart swapping via groups.

    // TODO: Alternate bindings. Should probably be implemented via KeyboardInput but will need support here.

    // TODO: Controller input.

    // TODO: Cancelling input checkers.

    public static void BindKey(Input input, Keys key)
    {
        KeyboardListeners[input].Rebind(key);
    }

    public static void Remove(Input input)
    {
        KeyboardListeners.Remove(input);
    }

    #region Getters

    public static bool GetPressed(Input input)
    {
        return KeyboardListeners[input].InputPressed;
    }

    public static bool GetReleased(Input input)
    {
        return KeyboardListeners[input].InputReleased;
    }

    public static bool GetHeld(Input input)
    {
        return KeyboardListeners[input].InputHeld;
    }

    public static int GetHeldSteps(Input input)
    {
        return KeyboardListeners[input].InputHeldSteps;
    }

    #endregion
}