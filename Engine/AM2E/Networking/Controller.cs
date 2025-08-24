using AM2E.Input;

namespace AM2E.Networking;

public class Controller
{
    // TODO: Should we have a separate controller for this instead?
    public bool puppet = true;
        
    internal NetCommand NetCommand { get; set; }
    internal NetCommand prevNetCommand { get; set; }

    public bool GetHeld(Enum input)
    {
        if (!puppet || NetCommand == null)
            return InputManager.GetHeld(input);

        return NetCommand.inputs[Convert.ToInt32(input)];
    }

    public bool GetPressed(Enum input)
    {
        if (!puppet || NetCommand == null || prevNetCommand == null)
            return InputManager.GetPressed(input);

        var i = Convert.ToInt32(input);
        return NetCommand.inputs[i] && !prevNetCommand.inputs[i];
    }

    public bool GetReleased(Enum input)
    {
        if (!puppet || NetCommand == null || prevNetCommand == null)
            return InputManager.GetReleased(input);
        
        var i = Convert.ToInt32(input);
        return !NetCommand.inputs[i] && prevNetCommand.inputs[i];
    }

}