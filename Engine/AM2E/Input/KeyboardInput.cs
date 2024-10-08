﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace AM2E.Control;

internal sealed class KeyboardInput : InputBase<Keys, KeyboardState>
{
    internal KeyboardInput(Keys input) : base(input) { }
    
    [JsonConstructor]
    public KeyboardInput(List<Keys> input) : base(input) { }
    
    protected override void Poll(KeyboardState state, Keys input) => ProcessInput(state.IsKeyDown(input));
}