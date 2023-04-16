using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AM2E.Control;

internal sealed class GamePadInput : InputBase<Buttons, GamePadState>
{
    internal GamePadInput(Buttons input) : base(input) { }

    protected override void Poll(GamePadState state, Buttons input)
    {
        switch (input)
        {
            // TODO: Process analog triggers?
            // Override thumbstick values so that we can apply our own deadzones properly.
            case Buttons.LeftThumbstickUp or Buttons.LeftThumbstickDown or Buttons.LeftThumbstickLeft
                or Buttons.LeftThumbstickRight:
                ProcessInput(GetThumbstickInput(input, state.ThumbSticks.Left, InputManager.LeftCenterDeadZone));
                break;
            case Buttons.RightThumbstickUp or Buttons.RightThumbstickDown or Buttons.RightThumbstickLeft
                or Buttons.RightThumbstickRight:
                ProcessInput(GetThumbstickInput(input, state.ThumbSticks.Right, InputManager.RightCenterDeadZone));
                break;
            // Default: process button
            default:
                ProcessInput(state.IsButtonDown(input));
                break;
        }
    }

    private bool GetThumbstickInput(Buttons button, Vector2 thumbStick, float deadZone)
    {
        var axis = ApplyDeadZone(thumbStick, deadZone);
        
        return button switch
        {
            Buttons.LeftThumbstickUp => (axis.Y > 0),
            Buttons.RightThumbstickUp => (axis.Y > 0),
            Buttons.LeftThumbstickDown => (axis.Y < 0),
            Buttons.RightThumbstickDown => (axis.Y < 0),
            Buttons.LeftThumbstickLeft => (axis.X < 0),
            Buttons.RightThumbstickLeft => (axis.X < 0),
            Buttons.LeftThumbstickRight => (axis.X > 0),
            Buttons.RightThumbstickRight => (axis.X > 0),
            _ => false
        };
    }

    private static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
    {
        return InputManager.CenterDeadZoneType switch
        {
            GamePadDeadZone.None => value,
            GamePadDeadZone.IndependentAxes => ExcludeAngularAxisDeadZone(ExcludeIndependentAxesDeadZone(value, deadZone), InputManager.AngularAxisDeadZone),
            GamePadDeadZone.Circular => ExcludeAngularAxisDeadZone(ExcludeCircularDeadZone(value, deadZone), InputManager.AngularAxisDeadZone),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static readonly double Left = Math.Atan2(0, -1);
    private static readonly double Right = Math.Atan2(0, 1);
    private static readonly double Up = Math.Atan2(1, 0);
    private static readonly double Down = Math.Atan2(-1, 0);

    private static Vector2 ExcludeAngularAxisDeadZone(Vector2 value, float deadZone)
    {
        if (MathHelper.IsApproximatelyZero(InputManager.AngularAxisDeadZone))
            return value;

        var radians = Microsoft.Xna.Framework.MathHelper.ToRadians(deadZone);
        var angle = Math.Atan2(value.Y, value.X);

        if (angle < Right + radians && angle > Right - radians)
            angle = Right;
        else if (angle < Up + radians && angle > Up - radians)
            angle = Up;
        else if (angle < Down + radians && angle > Down - radians)
            angle = Down;
        else if (angle > Math.PI - radians || angle < radians - Math.PI)
            angle = Left;
        else
        {
            const double PI_HALVES = Math.PI / 2;

            var signY = Math.Sign(value.Y);
            var offsetX = value.X > 0 ? 0 : (PI_HALVES * signY);
            
            // Don't touch this.
            // It's the secret sauce for making the diagonal deadzones not painful by mapping the smaller diagonal space into the full diagonal range.
            
            // Remove radians offset, remove pi halves offset
            angle = ((angle - radians * signY) - offsetX)
                    // Multiply to make the angle "expand" into the pi halves space
                    * PI_HALVES / (PI_HALVES - (radians * 2)) 
                    // Re-add pi halves offset
                    + offsetX;
        }
        
        Console.WriteLine(angle);

        var strength = value.Length();
        value.X = MathHelper.RoundToZero((float)Math.Cos(angle));
        value.Y = MathHelper.RoundToZero((float)Math.Sin(angle));

        return value * strength;
    }
    
    // Yes, this is just some existing MonoGame code from GamePadThumbSticks.cs.
    // Unfortunately, they haven't yet seen fit to expose methods for setting deadzone strengths, so I have to handle it all manually.
    // Might as well work with what they put in the framework instead of reinventing the wheel...
   
    private static Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
    {
        var originalLength = value.Length();
        if (originalLength <= deadZone)
            return Vector2.Zero;
        var newLength = (originalLength - deadZone) / (1f - deadZone);
        return value * (newLength / originalLength);
    }
    
    private static Vector2 ExcludeIndependentAxesDeadZone(Vector2 value, float deadZone)
    {
        return new Vector2(ExcludeAxisDeadZone(value.X, deadZone), ExcludeAxisDeadZone(value.Y, deadZone));
    }
    
    private static float ExcludeAxisDeadZone(float value, float deadZone)
    {
        if (value < -deadZone)
            value += deadZone;
        else if (value > deadZone)
            value -= deadZone;
        else
            return 0f;
        return value / (1f - deadZone);
    }
}