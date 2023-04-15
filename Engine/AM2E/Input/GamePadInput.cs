using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace AM2E.Control;

public sealed class GamePadInput : InputBase<Buttons, GamePadState>
{
    public GamePadInput(Buttons input) : base(input)
    {
    }

    public override void Poll(GamePadState state)
    {
        switch (Input)
        {
            // TODO: Process analog triggers?
            // Override thumbstick values so that we can apply our own deadzone properly.
            case Buttons.LeftThumbstickUp or Buttons.LeftThumbstickDown or Buttons.LeftThumbstickLeft
                or Buttons.LeftThumbstickRight:
                ProcessInput(ProcessLeftThumbstick(state));
                break;
            case Buttons.RightThumbstickUp or Buttons.RightThumbstickDown or Buttons.RightThumbstickLeft
                or Buttons.RightThumbstickRight:
                ProcessRightThumbstick(state);
                break;
            // Default: process button
            default:
                ProcessInput(state.IsButtonDown(Input));
                break;
        }
    }

    private void ProcessRightThumbstick(GamePadState state)
    {
        var axis = ApplyDeadZone(state.ThumbSticks.Right, InputManager.RightCenterDeadzone);
        
        switch (Input)
        {
            case Buttons.RightThumbstickUp:
                if (axis.Y > 0)
                    ProcessInput(true);
                break;
            case Buttons.RightThumbstickDown:
                if (axis.Y < 0)
                    ProcessInput(true);
                break;
            case Buttons.RightThumbstickLeft:
                if (axis.X < 0)
                    ProcessInput(true);
                break;
            case Buttons.RightThumbstickRight:
                if (axis.X > 0)
                    ProcessInput(true);
                break;
        }
    }

    private bool ProcessLeftThumbstick(GamePadState state)
    {
        var axis = ApplyDeadZone(state.ThumbSticks.Left, InputManager.LeftCenterDeadzone);

        Console.WriteLine(axis.X + ", " + axis.Y + " = " + state.ThumbSticks.Left.X + ", " + state.ThumbSticks.Left.Y);
        
        switch (Input)
        {
            case Buttons.LeftThumbstickUp:
                if (axis.Y > 0)
                    return true;
                break;
            case Buttons.LeftThumbstickDown:
                if (axis.Y < 0)
                    return true;
                break;
            case Buttons.LeftThumbstickLeft:
                if (axis.X < 0)
                    return true;
                break;
            case Buttons.LeftThumbstickRight:
                if (axis.X > 0)
                    return true;
                break;
        }

        return false;
    }

    private static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
    {
        return InputManager.CenterDeadZoneType switch
        {
            GamePadDeadZone.None => value,
            GamePadDeadZone.IndependentAxes => ExcludeIndependentAxesDeadZone(value, deadZone),
            GamePadDeadZone.Circular => ExcludeAngularDeadZone(ExcludeCircularDeadZone(value, deadZone), InputManager.DiagonalDeadZone),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static double left = Math.Atan2(0, -1);
    private static double right = Math.Atan2(0, 1);
    private static double up = Math.Atan2(1, 0);
    private static double down = Math.Atan2(-1, 0);

    private static Vector2 ExcludeAngularDeadZone(Vector2 value, float deadZone)
    {
        var radians = Microsoft.Xna.Framework.MathHelper.ToRadians(deadZone);
        var strength = value.Length();
        var angle = Math.Atan2(value.Y, value.X);

        if (angle < right + radians && angle > right - radians)
            angle = right;
        else if (angle < up + radians && angle > up - radians)
            angle = up;
        else if (angle < down + radians && angle > down - radians)
            angle = down;
        else if (angle > Math.PI - radians || angle < radians - Math.PI)
            angle = left;
        else
        {
            Console.WriteLine("angle: " + angle);

            var piHalves = Math.PI / 2;
            
            /*
             * angle = angle * piHalves / piHalves
             * angle = (angle * ((piHalves - radians * 2) / piHalves)); 
             */

            if (value.Y > 0)
            {
                angle -= radians;
                if (value.X > 0)
                {
                    angle = angle * piHalves / (piHalves - (radians * 2));
                }
                else if (value.X < 0)
                {
                    angle -= piHalves;
                    angle = angle * piHalves / (piHalves - (radians * 2));
                    angle += piHalves;
                }
            }
            else if (value.Y < 0)
            {
                angle += radians;
                if (value.X > 0)
                {
                    angle = angle * piHalves / (piHalves - (radians * 2));
                }
                else if (value.X < 0)
                {
                    angle += piHalves;
                    angle = angle * piHalves / (piHalves - (radians * 2));
                    angle -= piHalves;
                }
            }
            
            
            
            Console.WriteLine("new angle: " + angle);
        }
        
        
        return new Vector2(MathHelper.RoundToZero((float)Math.Cos(angle)), MathHelper.RoundToZero((float)Math.Sin(angle))) * strength;

        /*
        var angle = value.ToAngle();
        var radians = Microsoft.Xna.Framework.MathHelper.ToRadians(deadZone);

        if (value.X > 0 && angle < right + radians && angle > right - radians)
            angle = right;
        
        else if (value.X < 0 && angle < left + radians && angle > left - radians)
            angle = left;
        else if (value.Y > 0 && (angle < radians - Math.PI || angle > Math.PI - radians))
            angle = up;
        else if (value.Y < 0 && angle < radians && angle > -radians)
            angle = down;


        Console.WriteLine(angle);
        
        Console.WriteLine(Math.Sin(angle));

        var output = new Vector2((float)-Math.Sin(angle), (float)Math.Cos(angle));
        
        //Console.WriteLine(output.X + ", " + output.Y);
        
        return output;
        */
    }
    
    // Yes, this is just some existing MonoGame code from GamePadThumbSticks.cs.
    // Unfortunately, they haven't yet seen fit to expose methods for setting deadzone strengths, so I have to handle it all manually.
   
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