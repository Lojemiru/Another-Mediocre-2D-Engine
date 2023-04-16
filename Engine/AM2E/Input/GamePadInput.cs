using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AM2E.Control;

internal sealed class GamePadInput : InputBase<Buttons, GamePadState>
{
    internal GamePadInput(Buttons input) : base(input) { }

    protected override void Poll(GamePadState state, Buttons input)
    {
        // We have to handle some input types in a totally custom fashion - thumbsticks in particular.
        switch (input)
        {
            // Override thumbstick values so that we can apply our own dead zones properly.
            case Buttons.RightThumbstickUp or Buttons.RightThumbstickDown or Buttons.RightThumbstickLeft
                or Buttons.RightThumbstickRight:
                ProcessInput(GetThumbstickInput(input, InputManager.RightStick));
                break;
            case Buttons.LeftThumbstickUp or Buttons.LeftThumbstickDown or Buttons.LeftThumbstickLeft
                or Buttons.LeftThumbstickRight:
                ProcessInput(GetThumbstickInput(input, InputManager.LeftStick));
                break;
            // Default: process button
            default:
                ProcessInput(state.IsButtonDown(input));
                break;
        }
    }

    private static bool GetThumbstickInput(Buttons button, Vector2 axis)
    {
        return button switch
        {
            Buttons.LeftThumbstickUp => axis.Y > 0,
            Buttons.RightThumbstickUp => axis.Y > 0,
            Buttons.LeftThumbstickDown => axis.Y < 0,
            Buttons.RightThumbstickDown => axis.Y < 0,
            Buttons.LeftThumbstickLeft => axis.X < 0,
            Buttons.RightThumbstickLeft => axis.X < 0,
            Buttons.LeftThumbstickRight => axis.X > 0,
            Buttons.RightThumbstickRight => axis.X > 0,
            _ => false
        };
    }
}