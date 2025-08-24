using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace AM2E.Control;

internal sealed class MouseInput : InputBase<MouseButtons, MouseState>
{
    internal MouseInput(MouseButtons input) : base(input, InputType.Mouse) { }
    
    [JsonConstructor]
    public MouseInput(List<MouseButtons> inputs) : base(inputs, InputType.Mouse) { }
    
    private int wheelLast;

    internal override void Update(MouseState state)
    {
        base.Update(state);
        wheelLast = state.ScrollWheelValue;
    }

    protected override void Poll(MouseState state, MouseButtons input)
    {
        // Don't process mouse data if it's not in focus... because MonoGame does that, apparently.
        if (!EngineCore._graphics.GraphicsDevice.Viewport.Bounds.Contains(state.X, state.Y))
            return;

        // Figure out our input's state.
        var buttonBool = input switch
        {
            MouseButtons.Left => state.LeftButton == ButtonState.Pressed,
            MouseButtons.Middle => state.MiddleButton == ButtonState.Pressed,
            MouseButtons.Right => state.RightButton == ButtonState.Pressed,
            MouseButtons.XButton1 => state.XButton1 == ButtonState.Pressed,
            MouseButtons.XButton2 => state.XButton2 == ButtonState.Pressed,
            MouseButtons.WheelUp => (wheelLast > state.ScrollWheelValue),
            MouseButtons.WheelDown => (wheelLast < state.ScrollWheelValue),
            MouseButtons.None => false,
            _ => throw new ArgumentOutOfRangeException()
        };

        ProcessInput(buttonBool);
    }
}