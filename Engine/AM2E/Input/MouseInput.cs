using System;
using Microsoft.Xna.Framework.Input;

namespace AM2E.Control;

internal sealed class MouseInput : InputBase<MouseButton, MouseState>
{
    internal MouseInput(MouseButton input) : base(input) { }
    private int wheelLast;

    protected override void Poll(MouseState state, MouseButton input)
    {
        if (!EngineCore._graphics.GraphicsDevice.Viewport.Bounds.Contains(state.X, state.Y))
            return;

        wheelLast = state.ScrollWheelValue;
        
        var buttonBool = input switch
        {
            MouseButton.Left => state.LeftButton == ButtonState.Pressed,
            MouseButton.Middle => state.MiddleButton == ButtonState.Pressed,
            MouseButton.Right => state.RightButton == ButtonState.Pressed,
            MouseButton.XButton1 => state.XButton1 == ButtonState.Pressed,
            MouseButton.XButton2 => state.XButton2 == ButtonState.Pressed,
            MouseButton.WheelUp => (wheelLast > state.ScrollWheelValue),
            MouseButton.WheelDown => (wheelLast < state.ScrollWheelValue),
            MouseButton.None => false,
            _ => throw new ArgumentOutOfRangeException()
        };

        ProcessInput(buttonBool);
    }
}