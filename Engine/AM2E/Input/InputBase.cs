using Newtonsoft.Json;

namespace AM2E.Control;

internal abstract class InputBase<TInput, TState>
{
    [JsonProperty("i")]
    public readonly List<TInput> Inputs = new();

    protected InputType InputType;
    
    protected InputBase(List<TInput> inputs, InputType inputType)
    {
        Inputs = inputs;
        InputType = inputType;
    }

    protected InputBase(TInput input, InputType inputType)
    {
        Inputs.Add(input);
        InputType = inputType;
    }

    internal bool InputReleased { get; private set; } = false;
    internal bool InputPressed { get; private set; } = false;
    private bool inputPressedLast = false;
    internal bool InputHeld { get; private set; } = false;

    internal virtual void Update(TState state)
    {
        InputHeld = false;

        foreach (var input in Inputs)
        {
            Poll(state, input);
        }

        InputPressed = InputHeld && !inputPressedLast;

        InputReleased = !InputHeld && inputPressedLast;

        inputPressedLast = InputHeld;
        
        if (InputHeld)
            InputManager.LastReadInputType = InputType;
    }
    
    protected void ProcessInput(bool input)
    {
        if (input)
            InputHeld = true;
    }

    protected abstract void Poll(TState state, TInput input);

    internal int AddAlternateBinding(TInput input)
    {
        Inputs.Add(input);
        return Inputs.Count;
    }
    
    internal void Rebind(TInput input, int index = 0)
    {
        Inputs[index] = input;
    }
}