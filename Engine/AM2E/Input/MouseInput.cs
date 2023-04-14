using System;
using Microsoft.Xna.Framework.Input;

namespace AM2E.Control;

public sealed class MouseInput : InputBase<MouseButton, MouseState>
{
    public MouseInput(MouseButton input) : base(input) { }
    private int wheelLast;

    public override void Poll(MouseState state)
    {
        if (!EngineCore._graphics.GraphicsDevice.Viewport.Bounds.Contains(state.X, state.Y))
        {
            ProcessInput(false);
            return;
        }
        
        var buttonBool = Input switch
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

        wheelLast = state.ScrollWheelValue;
        
        ProcessInput(buttonBool);
    }
}