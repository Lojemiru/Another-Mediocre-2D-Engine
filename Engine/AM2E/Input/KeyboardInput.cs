using Microsoft.Xna.Framework.Input;

namespace AM2E.Control;

public sealed class KeyboardInput : InputBase<Keys, KeyboardState>
{
    public KeyboardInput(Keys key) : base(key) { }
    
    public override void Poll(KeyboardState state) => ProcessInput(state.IsKeyDown(Input));
}