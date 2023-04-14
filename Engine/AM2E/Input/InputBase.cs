namespace AM2E.Control;

public abstract class InputBase<TInput, TState>
{
    protected TInput Input;
    
    protected InputBase(TInput input)
    {
        Input = input;
    }

    public bool InputReleased { get; protected set; } = false;
    public bool InputPressed { get; protected set; } = false;
    protected bool InputPressedLast = false;
    public bool InputHeld { get; protected set; } = false;
    public int InputHeldSteps { get; protected set; } = 0;
    
    protected void ProcessInput(bool input)
    {
        InputHeld = input;

        if (InputHeld) InputHeldSteps++;
        else InputHeldSteps = 0;

        InputPressed = InputHeld && !InputPressedLast;

        InputReleased = !InputHeld && InputPressedLast;

        InputPressedLast = InputHeld;
    }

    public abstract void Poll(TState state);
    
    public void Rebind(TInput input)
    {
        Input = input;
    }
}