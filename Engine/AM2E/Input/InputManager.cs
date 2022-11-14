using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace AM2E.Control
{
    public static class InputManager
    {
        private static Dictionary<Input, KeyboardInput> keyboardListeners = new Dictionary<Input, KeyboardInput>();
        
        static InputManager()
        {
            foreach (Input input in Enum.GetValues(typeof(Input)))
            {
                keyboardListeners.Add(input, new KeyboardInput(Keys.None));
            }
        }

        public static void Update()
        {
            var keyboardState = Keyboard.GetState();
            Dictionary<Input, KeyboardInput>.ValueCollection valColl = keyboardListeners.Values;
            foreach (KeyboardInput listener in valColl)
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
            keyboardListeners[input].Rebind(key);
        }

        public static void Remove(Input input)
        {
            keyboardListeners.Remove(input);
        }

        #region Getters

        public static bool GetPressed(Input input)
        {
            return keyboardListeners[input].InputPressed;
        }

        public static bool GetReleased(Input input)
        {
            return keyboardListeners[input].InputReleased;
        }

        public static bool GetHeld(Input input)
        {
            return keyboardListeners[input].InputHeld;
        }

        public static int GetHeldSteps(Input input)
        {
            return keyboardListeners[input].InputHeldSteps;
        }

        #endregion
    }
}
