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
    
    private const double PI_HALVES = Math.PI / 2;
    private const double PI_FOURTHS = Math.PI / 4;

    private static Vector2 ExcludeAngularAxisDeadZone(Vector2 value, float deadZone)
    {
        // Exit immediately if the angular axis dead zone is of no concern.
        if (MathHelper.IsApproximatelyZero(InputManager.AngularAxisDeadZone))
            return value;
        
        var angle = Math.Atan2(value.Y, value.X);
        var angleAbs = Math.Abs(angle);
        // Do NOT use Math.Sign(x) here; we want to ensure this is never 0.
        var angleSign = angle < 0 ? -1 : 1;
        
        var deadZoneRadians = Microsoft.Xna.Framework.MathHelper.ToRadians(deadZone);
        
        // Get radial offset - how much we need to subtract to bring the angle relative to 0.
        var radialOffset = (PI_HALVES * (int)((angleAbs + PI_FOURTHS) / PI_HALVES));

        // If the angle minus its radial offset is within our dead zone, snap it to the radial offset!
        if (Math.Abs(angleAbs - radialOffset) < deadZoneRadians)
            angle = radialOffset * angleSign;
        // Otherwise, we need to scale the input value into the full input range so that we do not lose possible angle values.
        else
        {
            radialOffset = value.X > 0 ? 0 : PI_HALVES;
            
            // I suggest that you do not touch this equation. I'm quite proud of it.
            
            // Remove radians offset, remove pi halves offset.
            angle = ((angleAbs - deadZoneRadians - radialOffset)
                    // Multiply to make the angle "expand" into the pi halves space.
                    * PI_HALVES / (PI_HALVES - (deadZoneRadians * 2))
                    // Re-add pi halves offset.
                    + radialOffset)
                    // Reapply sign.
                    * angleSign;
        }
        
        // Maintain input strength, but give the value our new X and Y components.
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