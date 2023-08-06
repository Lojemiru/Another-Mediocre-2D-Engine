using GameContent.EngineConfig;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using AM2E.Graphics;
using Microsoft.Xna.Framework;

namespace AM2E.Control;

#region Design Notes

/*
 * With the exception of mouse X/Y position, all inputs (including analog like thumbsticks/triggers) are processed into
 *      a binary state. This is the simplest solution for most end users in a traditional 2D context, especially when
 *      we're applying dead zones. However, there are several reasons that the end user may still want raw analog input,
 *      so I have provided a means to read this input for thumbsticks and analog triggers.
 *
 * Alternative bindings are also an important concept. These allow for the simple assignment of multiple buttons to the
 *      same user-defined Input; a common use case for this is assigning both the d-pad and left thumbstick to movement.
 *
 * MonoGame does not allow you to modify the applied dead zone values (presumably an XNA leftover because Microsoft
 *      assumed nobody would use anything other than an Xbox controller). As such, I've copied their dead zone code
 *      (and license, as per its terms) into here so that user-defined dead zone values may be utilized.
 *
 * I've also added another type of dead zone that should be used in more 2D games: an angular axis dead zone.
 *      It determines how many degrees away from any of the four cardinal directions should still be counted as being
 *      that direction alone; this is a way to prevent over-sensitive controllers from drifting off the intended
 *      direction too easily.
 *
 * MonoGame does not allow you to set a duration for controller rumble. Instead, it picks a default value based on the
 *      platform (or something similar, I didn't dig too deep). Anyway, point is: the slightly odd rumble setup here is
 *      necessary to let us set custom durations.
 */

#endregion

public static class InputManager
{
    private static readonly Dictionary<Input, KeyboardInput> KeyboardListeners = new();
    private static readonly Dictionary<Input, MouseInput> MouseListeners = new();
    private static readonly Dictionary<Input, GamePadInput> GamePadListeners = new();
    
    private const double PI_HALVES = Math.PI / 2;
    private const double PI_FOURTHS = Math.PI / 4;

    public static float RightCenterDeadZone = 0.1f;
    public static float LeftCenterDeadZone = 0.1f;
    public static GamePadDeadZone CenterDeadZoneType = GamePadDeadZone.Circular;
    public static float AngularAxisDeadZone = 15f;

    // TODO: Make this swap dynamically?
    public static readonly int GamePadIndex = 0;
    private static int vibrationTicks = 0;
    private static float leftMotorVibration = 0;
    private static float rightMotorVibration = 0;

    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }
    public static Vector2 RightStick { get; private set; }
    public static Vector2 LeftStick { get; private set; }
    public static float RightTrigger { get; private set; }
    public static float LeftTrigger { get; private set; }

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
        // Keyboard
        var keyboardState = Keyboard.GetState();
        foreach (var listener in KeyboardListeners.Values)
        {
            listener.Update(keyboardState);
        }
        
        // Mouse
        var mouseState = Mouse.GetState();
        
        MouseX = Math.Clamp(Camera.BoundLeft + (int)((mouseState.X - Renderer.ApplicationSpace.X) * ((float)Renderer.GameWidth / Renderer.ApplicationSpace.Width)), Camera.BoundLeft, Camera.BoundRight);
        MouseY = Math.Clamp(Camera.BoundTop + (int)((mouseState.Y - Renderer.ApplicationSpace.Y) * ((float)Renderer.GameHeight / Renderer.ApplicationSpace.Height)), Camera.BoundTop, Camera.BoundBottom);
        
        foreach (var listener in MouseListeners.Values)
        {
            listener.Update(mouseState);
        }
        
        // GamePad
        var gamePadState = GamePad.GetState(GamePadIndex);
        
        RightStick = ApplyDeadZone(gamePadState.ThumbSticks.Right, RightCenterDeadZone);
        LeftStick = ApplyDeadZone(gamePadState.ThumbSticks.Left, LeftCenterDeadZone);
        
        RightTrigger = gamePadState.Triggers.Right;
        LeftTrigger = gamePadState.Triggers.Left;
        
        foreach (var listener in GamePadListeners.Values)
        {
            listener.Update(gamePadState);
        }
        
        StopVibration();
        
        if (vibrationTicks > 0)
        {
            GamePad.SetVibration(GamePadIndex, leftMotorVibration, rightMotorVibration);
            vibrationTicks--;
        }
    }

    // TODO: Rebinding. Needs to handle smart swapping via groups.

    #region Binding
    
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
    
    #endregion

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
        // TODO: this is probably bad practice and not net-supported, nuke it
        return KeyboardListeners[input].InputHeldSteps | MouseListeners[input].InputHeldSteps | GamePadListeners[input].InputHeldSteps;
    }

    #endregion
    
    #region Vibration

    public static void SetVibration(int ticks, float strength) => SetVibration(ticks, strength, strength);

    public static void SetVibration(int ticks, float leftMotor, float rightMotor)
    {
        vibrationTicks = ticks;
        leftMotorVibration = leftMotor;
        rightMotorVibration = rightMotor;
    }

    public static void StopVibration() => GamePad.SetVibration(GamePadIndex, 0, 0);

    #endregion
    
    #region DeadZones
    
    internal static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
    {
        return InputManager.CenterDeadZoneType switch
        {
            GamePadDeadZone.None => value,
            GamePadDeadZone.IndependentAxes => ExcludeAngularAxisDeadZone(ExcludeIndependentAxesDeadZone(value, deadZone), AngularAxisDeadZone),
            GamePadDeadZone.Circular => ExcludeAngularAxisDeadZone(ExcludeCircularDeadZone(value, deadZone), AngularAxisDeadZone),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static Vector2 ExcludeAngularAxisDeadZone(Vector2 value, float deadZone)
    {
        // Exit immediately if the angular axis dead zone is of no concern.
        if (MathHelper.IsApproximatelyZero(AngularAxisDeadZone))
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

    // Yes, the following methods are just some existing MonoGame code from GamePadThumbSticks.cs.
    // Unfortunately, they haven't yet seen fit to expose methods for setting deadzone strengths (probably an XNA holdover), so I have to handle it all in this class.
    // Might as well work with what they put in the framework instead of reinventing the wheel...

    // Anyway, here's the license:
    #region MonoGame License
    /*
        Microsoft Public License (Ms-PL)
        MonoGame - Copyright © 2009-2021 The MonoGame Team

        All rights reserved.

        This license governs use of the accompanying software. If you use the software,
        you accept this license. If you do not accept the license, do not use the
        software.

        1. Definitions

        The terms "reproduce," "reproduction," "derivative works," and "distribution"
        have the same meaning here as under U.S. copyright law.

        A "contribution" is the original software, or any additions or changes to the
        software.

        A "contributor" is any person that distributes its contribution under this
        license.

        "Licensed patents" are a contributor's patent claims that read directly on its
        contribution.

        2. Grant of Rights

        (A) Copyright Grant- Subject to the terms of this license, including the
        license conditions and limitations in section 3, each contributor grants you a
        non-exclusive, worldwide, royalty-free copyright license to reproduce its
        contribution, prepare derivative works of its contribution, and distribute its
        contribution or any derivative works that you create.

        (B) Patent Grant- Subject to the terms of this license, including the license
        conditions and limitations in section 3, each contributor grants you a
        non-exclusive, worldwide, royalty-free license under its licensed patents to
        make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
        its contribution in the software or derivative works of the contribution in the
        software.

        3. Conditions and Limitations

        (A) No Trademark License- This license does not grant you rights to use any
        contributors' name, logo, or trademarks.

        (B) If you bring a patent claim against any contributor over patents that you
        claim are infringed by the software, your patent license from such contributor
        to the software ends automatically.

        (C) If you distribute any portion of the software, you must retain all
        copyright, patent, trademark, and attribution notices that are present in the
        software.

        (D) If you distribute any portion of the software in source code form, you may
        do so only under this license by including a complete copy of this license with
        your distribution. If you distribute any portion of the software in compiled or
        object code form, you may only do so under a license that complies with this
        license.

        (E) The software is licensed "as-is." You bear the risk of using it. The
        contributors give no express warranties, guarantees or conditions. You may have
        additional consumer rights under your local laws which this license cannot
        change. To the extent permitted under your local laws, the contributors exclude
        the implied warranties of merchantability, fitness for a particular purpose and
        non-infringement.

        -------------------------------------------------------------------------------

        The MIT License (MIT)
        Portions Copyright © The Mono.Xna Team

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
     */
    #endregion
    
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
    
    #endregion
}