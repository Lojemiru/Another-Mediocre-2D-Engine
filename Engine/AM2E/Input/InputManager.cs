using Microsoft.Xna.Framework.Input;
using AM2E.Graphics;
using AM2E.IO;
using Microsoft.Xna.Framework;

namespace AM2E.Control;

#region Design Notes

/*
 * [Analog vs. binary inputs]
 * With the exception of mouse X/Y position, all inputs (including analog like thumb sticks/triggers) are processed into
 *      a binary state. This is the simplest solution for most end users in a traditional 2D context, especially when
 *      we're applying dead zones. However, there are several reasons that the end user may still want raw analog input,
 *      so I have provided a means to read this input for thumb sticks and analog triggers.
 *
 * [Bindings]
 * Alternative bindings are an important concept. These allow for the simple assignment of multiple buttons to the same
 *      user-defined input; a common use case for this is assigning both the d-pad and left thumbstick to movement. I've
 *      dealt with this too many times to not support it out of the box.
 *
 * Rebinding is something I've tussled with for far longer than I'd like to admit. Not the actual key indexes being
 *      changed, but setting up a sane system for automatically swapping conflicting keys. The problem is that in a lot
 *      of situations you NEED keys to conflict - you need "groups" of keys that will auto-swap with each other, but not
 *      with keys outside their group. A common example is having separate bindings for menu controls; you probably want
 *      these to be able to overlap with gameplay controls like jumping and attacking. I've provided this with the
 *      "rebinding groups" functionality, and I hope it proves useful.
 *
 * [Dead zones]
 * MonoGame does not allow you to modify the applied dead zone values (presumably an XNA leftover because Microsoft
 *      assumed nobody would use anything other than an Xbox controller). As such, I've copied their dead zone code
 *      (and license, as per its terms) into here so that user-defined dead zone values may be utilized.
 * 
 * I've also added another type of dead zone that should be used in more 2D games: an angular axis dead zone.
 *      It determines how many degrees away from any of the four cardinal directions should still be counted as being
 *      that direction alone; this is a way to prevent over-sensitive controllers from drifting off the intended
 *      direction too easily.
 *
 * [Haptics]
 * MonoGame does not allow you to set a duration for controller rumble. Instead, it picks a default value based on the
 *      platform (or something similar, I didn't dig too deep). Anyway, point is: the slightly odd rumble setup here is
 *      necessary to let us set custom durations.
 */

#endregion

public static class InputManager
{
    internal static readonly Dictionary<string, KeyboardInput> KeyboardListeners = new();
    internal static readonly Dictionary<string, MouseInput> MouseListeners = new();
    internal static readonly Dictionary<string, GamePadInput> GamePadListeners = new();
    internal static readonly Dictionary<string, string> RebindGroupMappings = new();
    
    private const double PI_HALVES = Math.PI / 2;
    private const double PI_FOURTHS = Math.PI / 4;

    public static float RightCenterDeadZone = 0.1f;
    public static float LeftCenterDeadZone = 0.1f;
    public static GamePadDeadZone CenterDeadZoneType = GamePadDeadZone.Circular;
    public static float AngularAxisDeadZone = 15f;
    
    /// <summary>
    /// Whether to accept controller input when the window is not focused.
    /// </summary>
    public static bool AcceptBackgroundInput = true;

    public static InputType LastReadInputType { get; internal set; } = InputType.Keyboard;
    
    public static int GamePadIndex { get; private set; } = 2;
    private static int vibrationTicks = 0;
    private static float leftMotorVibration = 0;
    private static float rightMotorVibration = 0;

    public static bool AcceptUpdates { get; set; } = true;
    public static bool UpdateVibration { get; set; } = true;

    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }
    public static Vector2 RightStick { get; private set; }
    public static Vector2 LeftStick { get; private set; }
    public static float RightTrigger { get; private set; }
    public static float LeftTrigger { get; private set; }

    internal static Type EnumType;

    private static GamePadState[] connectedPads;

    internal static void Initialize(Type enumType)
    {
        EnumType = enumType;
        foreach (var input in Enum.GetNames(enumType))
        {
            KeyboardListeners.Add(input, new KeyboardInput(Keys.None));
            MouseListeners.Add(input, new MouseInput(MouseButtons.None));
            GamePadListeners.Add(input, new GamePadInput(Buttons.None));
            RebindGroupMappings[input] = "";
        }

        connectedPads = new GamePadState[GamePad.MaximumGamePadCount];
    }

    private static InputSerialization Serialize()
    {
        return new InputSerialization(KeyboardListeners, MouseListeners, GamePadListeners, RightCenterDeadZone, 
            LeftCenterDeadZone, AngularAxisDeadZone);
    }
    
    public static void Write(string name)
    {
        LocalStorage.Write(name, Serialize());
    }
    
    public static void WriteAsync(string name, Action callback = null)
    {
        LocalStorage.WriteAsync(name, Serialize(), callback);
    }

    public static void LoadFrom(string name)
    {
        if (!LocalStorage.Exists(name))
            Write(name);
        
        LocalStorage.Read(name, out InputSerialization s);
        
        foreach (var pair in s.KeyboardListeners.Where(pair => KeyboardListeners.ContainsKey(pair.Key)))
            KeyboardListeners[pair.Key] = pair.Value;
        
        foreach (var pair in s.MouseListeners.Where(pair => MouseListeners.ContainsKey(pair.Key)))
            MouseListeners[pair.Key] = pair.Value;
        
        foreach (var pair in s.GamePadListeners.Where(pair => GamePadListeners.ContainsKey(pair.Key)))
            GamePadListeners[pair.Key] = pair.Value;

        RightCenterDeadZone = s.RightCenterDeadZone;
        LeftCenterDeadZone = s.LeftCenterDeadZone;
        AngularAxisDeadZone = s.AngularAxisDeadZone;
    }

    internal static void Update()
    {
        if (AcceptUpdates)
        {
            // Keyboard
            var keyboardState = Keyboard.GetState();
            foreach (var listener in KeyboardListeners.Values)
            {
                listener.Update(keyboardState);
            }

            // Mouse
            var mouseState = Mouse.GetState();

            MouseX = Math.Clamp(
                Camera.BoundLeft + (int)((mouseState.X - Renderer.ApplicationSpace.X) *
                                         ((float)Renderer.GameWidth / Renderer.ApplicationSpace.Width)),
                Camera.BoundLeft, Camera.BoundRight);
            MouseY = Math.Clamp(
                Camera.BoundTop + (int)((mouseState.Y - Renderer.ApplicationSpace.Y) *
                                        ((float)Renderer.GameHeight / Renderer.ApplicationSpace.Height)),
                Camera.BoundTop, Camera.BoundBottom);

            if (EngineCore.WindowFocused)
            {
                foreach (var listener in MouseListeners.Values)
                {
                    listener.Update(mouseState);
                }
            }

            // Dynamically swap pad state.
            for (var i = 0; i < connectedPads.Length; i++)
            {
                var state = GamePad.GetState(i);
                
                if (state.IsConnected && !connectedPads[i].IsConnected)
                    GamePadIndex = i;
                
                connectedPads[i] = state;
            }

            // Dynamically pick next controller when current one is unplugged.
            // This WILL gracefully fail when no controller is connected and just not change the index.
            if (!connectedPads[GamePadIndex].IsConnected)
            {
                for (var i = 0; i < connectedPads.Length; i++)
                    if (connectedPads[i].IsConnected)
                        GamePadIndex = i;
            }
            
            var gamePadState = connectedPads[GamePadIndex];

            RightStick = ApplyDeadZone(gamePadState.ThumbSticks.Right, RightCenterDeadZone);
            LeftStick = ApplyDeadZone(gamePadState.ThumbSticks.Left, LeftCenterDeadZone);

            RightTrigger = gamePadState.Triggers.Right;
            LeftTrigger = gamePadState.Triggers.Left;

            foreach (var listener in GamePadListeners.Values)
            {
                listener.Update(gamePadState);
            }
        }

        StopVibration();
        
        if (UpdateVibration && vibrationTicks > 0)
        {
            GamePad.SetVibration(GamePadIndex, leftMotorVibration, rightMotorVibration);
            vibrationTicks--;
        }
    }

    private static void ValidateInputExists(string inputName)
    {
        if (!KeyboardListeners.ContainsKey(inputName))
            throw new ArgumentException("Input name \"" + inputName + "\" is invalid!");
    }

    #region Binding

    public static void BindKey(Enum input, Keys key, int index = 0)
        => BindKey(input.ToString(), key, index);

    public static void BindKey(string input, Keys key, int index = 0)
    {
        ValidateInputExists(input);
        KeyboardListeners[input].Rebind(key, index);
    }

    public static void BindMouseButton(Enum input, MouseButtons mouseButtons, int index = 0)
        => BindMouseButton(input.ToString(), mouseButtons, index);
    
    public static void BindMouseButton(string input, MouseButtons mouseButtons, int index = 0)
    {
        ValidateInputExists(input);
        MouseListeners[input].Rebind(mouseButtons, index);
    }

    public static void BindGamePadButton(Enum input, Buttons button, int index = 0)
        => BindGamePadButton(input.ToString(), button, index);
    
    public static void BindGamePadButton(string input, Buttons button, int index = 0)
    {
        ValidateInputExists(input);
        GamePadListeners[input].Rebind(button, index);
    }

    public static int BindAlternateKey(Enum input, Keys key)
        => BindAlternateKey(input.ToString(), key);
    
    public static int BindAlternateKey(string input, Keys key)
    {
        ValidateInputExists(input);
        return KeyboardListeners[input].AddAlternateBinding(key);
    }

    public static int BindAlternateMouseButton(Enum input, MouseButtons mouseButtons)
        => BindAlternateMouseButton(input.ToString(), mouseButtons);
    
    public static int BindAlternateMouseButton(string input, MouseButtons mouseButtons)
    {
        ValidateInputExists(input);
        return MouseListeners[input].AddAlternateBinding(mouseButtons);
    }

    public static int BindAlternateGamePadButton(Enum input, Buttons button)
        => BindAlternateGamePadButton(input.ToString(), button);
    
    public static int BindAlternateGamePadButton(string input, Buttons button)
    {
        ValidateInputExists(input);
        return GamePadListeners[input].AddAlternateBinding(button);
    }
    
    #endregion

    #region Rebinding

    // Yes, I know that these methods have a large copypasted loop.
    // No, I can't be bothered to find a way to cleanly make it generic. If you're here and annoyed, please open a PR.
    
    public static void RebindKey(Enum inputName, Keys key, int index = 0)
        => RebindKey(inputName.ToString(), key, index);
    
    public static void RebindKey(string inputName, Keys key, int index = 0)
    {
        var groupName = RebindGroupMappings[inputName];
        var oldKey = KeyboardListeners[inputName].Inputs[index];

        // Search group mappings index...
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var input in RebindGroupMappings)
        {
            // Skip to next loop if it doesn't use our group.
            if (input.Value != groupName)
                continue;
                
            var targetListener = KeyboardListeners[input.Key];
                    
            // Otherwise, search each binding layer in the listener...
            for (var i = 0; i < targetListener.Inputs.Count; i++)
            {
                // ...and, if it matches our new key, rebind it to use our old key!
                if (targetListener.Inputs[i] == key)
                    targetListener.Rebind(oldKey, i);
            }
        }
        
        KeyboardListeners[inputName].Rebind(key, index);
    }

    public static void RebindMouseButton(Enum inputName, MouseButtons mouseButtons, int index = 0)
        => RebindMouseButton(inputName.ToString(), mouseButtons, index);
    
    public static void RebindMouseButton(string inputName, MouseButtons mouseButtons, int index = 0)
    {
        var groupName = RebindGroupMappings[inputName];
        var oldButton = MouseListeners[inputName].Inputs[index];

        // Search group mappings index...
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var input in RebindGroupMappings)
        {
            // Skip to next loop if it doesn't use our group.
            if (input.Value != groupName)
                continue;
                
            var targetListener = MouseListeners[input.Key];
                    
            // Otherwise, search each binding layer in the listener...
            for (var i = 0; i < targetListener.Inputs.Count; i++)
            {
                // ...and, if it matches our new button, rebind it to use our old button!
                if (targetListener.Inputs[i] == mouseButtons)
                    targetListener.Rebind(oldButton, i);
            }
        }
        
        MouseListeners[inputName].Rebind(mouseButtons, index);
    }

    public static void RebindGamePadButton(Enum inputName, Buttons button, int index = 0)
        => RebindGamePadButton(inputName.ToString(), button, index);
    
    public static void RebindGamePadButton(string inputName, Buttons button, int index = 0)
    {
        var groupName = RebindGroupMappings[inputName];
        var oldButton = GamePadListeners[inputName].Inputs[index];

        // Search group mappings index...
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var input in RebindGroupMappings)
        {
            // Skip to next loop if it doesn't use our group.
            if (input.Value != groupName)
                continue;
                
            var targetListener = GamePadListeners[input.Key];
                    
            // Otherwise, search each binding layer in the listener...
            for (var i = 0; i < targetListener.Inputs.Count; i++)
            {
                // ...and, if it matches our new button, rebind it to use our old button!
                if (targetListener.Inputs[i] == button)
                    targetListener.Rebind(oldButton, i);
            }
        }
        
        GamePadListeners[inputName].Rebind(button, index);
    }
    
    #endregion
    
    #region Rebinding Groups

    public static void AddInputToGroup(Enum inputName, Enum groupName)
        => AddInputToGroup(inputName.ToString(), groupName.ToString());

    public static void AddInputToGroup(string inputName, string groupName)
    {
        // Check for invalid input name...
        ValidateInputExists(inputName);
        
        // Then add the bind if we don't already have it in this group.
        if (!RebindGroupMappings[inputName].Equals(groupName))
            RebindGroupMappings[inputName] = groupName;
    }

    public static void RemoveInputFromGroup(Enum inputName)
        => RemoveInputFromGroup(inputName.ToString());
    
    public static void RemoveInputFromGroup(string inputName)
    {
        // Check for invalid group and input names...
        ValidateInputExists(inputName);
        
        // Then remove the bind.
        RebindGroupMappings[inputName] = null;
    }
    
    #endregion
    
    #region Getters

    public static bool GetPressed(Enum input)
        => GetPressed(input.ToString());
    
    public static bool GetPressed(string input)
    {
        ValidateInputExists(input);
        return KeyboardListeners[input].InputPressed | MouseListeners[input].InputPressed | GamePadListeners[input].InputPressed;
    }

    public static bool GetPressedCancelling(Enum input, Enum cancellingInput)
        => GetPressedCancelling(input.ToString(), cancellingInput.ToString());
    
    public static bool GetPressedCancelling(string input, string cancellingInput)
    {
        ValidateInputExists(input);
        ValidateInputExists(cancellingInput);
        return GetPressed(input) && !GetHeld(cancellingInput);
    }

    public static bool GetReleased(Enum input) 
        => GetReleased(input.ToString());
    
    public static bool GetReleased(string input)
    {
        ValidateInputExists(input);
        return KeyboardListeners[input].InputReleased | MouseListeners[input].InputReleased | GamePadListeners[input].InputReleased;
    }

    public static bool GetReleasedCancelling(Enum input, Enum cancellingInput)
        => GetReleasedCancelling(input.ToString(), cancellingInput.ToString());
    
    public static bool GetReleasedCancelling(string input, string cancellingInput)
    {
        ValidateInputExists(input);
        ValidateInputExists(cancellingInput);
        return GetReleased(input) && !GetHeld(cancellingInput);
    }

    public static bool GetHeld(Enum input)
        => GetHeld(input.ToString());
    
    public static bool GetHeld(string input)
    {
        ValidateInputExists(input);
        return KeyboardListeners[input].InputHeld | MouseListeners[input].InputHeld | GamePadListeners[input].InputHeld;
    }

    public static bool GetHeldCancelling(Enum input, Enum cancellingInput)
        => GetHeldCancelling(input.ToString(), cancellingInput.ToString());
    
    public static bool GetHeldCancelling(string input, string cancellingInput)
    {
        ValidateInputExists(input);
        ValidateInputExists(cancellingInput);
        return GetHeld(input) && !GetHeld(cancellingInput);
    }

    #endregion
    
    #region Binding Getters

    public static Keys GetBoundKey(Enum input, int index = 0)
        => GetBoundKey(input.ToString(), index);
    
    public static Keys GetBoundKey(string input, int index = 0)
    {
        ValidateInputExists(input);
        return KeyboardListeners[input].Inputs[index];
    }

    public static Buttons GetBoundGamePadButton(Enum input, int index = 0)
        => GetBoundGamePadButton(input.ToString(), index);

    public static Buttons GetBoundGamePadButton(string input, int index = 0)
    {
        ValidateInputExists(input);
        return GamePadListeners[input].Inputs[index];
    }

    public static MouseButtons GetBoundMouseButton(Enum input, int index = 0)
        => GetBoundMouseButton(input.ToString(), index);
    
    public static MouseButtons GetBoundMouseButton(string input, int index = 0)
    {
        ValidateInputExists(input);
        return MouseListeners[input].Inputs[index];
    }
    
    #endregion
    
    #region Vibration

    public static void SetVibration(int ticks, float strength) 
        => SetVibration(ticks, strength, strength);

    public static void SetVibration(int ticks, float leftMotor, float rightMotor)
    {
        vibrationTicks = ticks;
        leftMotorVibration = leftMotor;
        rightMotorVibration = rightMotor;
    }

    public static void StopVibration() 
        => GamePad.SetVibration(GamePadIndex, 0, 0);

    #endregion
    
    #region DeadZones
    
    internal static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
    {
        return CenterDeadZoneType switch
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