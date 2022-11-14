using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Control
{
    public class KeyboardInput
    {
        public bool InputReleased { get; private set; } = false;
        public bool InputPressed { get; private set; } = false;
        private bool inputPressedLast = false;
        public bool InputHeld { get; private set; } = false;
        public int InputHeldSteps { get; private set; } = 0;
        public Keys Key { get; private set; }

        public KeyboardInput(Keys key)
        {
            Key = key;
        }

        public void Poll(KeyboardState state)
        {
            InputHeld = state.IsKeyDown(Key);
            
            if (InputHeld) InputHeldSteps++;
            else InputHeldSteps = 0;

            InputPressed = InputHeld && !inputPressedLast;

            InputReleased = !InputHeld && inputPressedLast;

            inputPressedLast = InputHeld;
        }

        public void Rebind(Keys key)
        {
            Key = key;
        }
    }
}
