using GameContent.EngineConfig;
using AM2E.Control;

namespace AM2E.Networking;

public class Controller
{
    // TODO: Should we have a separate controller for this instead?
    public bool puppet = true;
        
    internal NetCommand NetCommand { get; set; }
    internal NetCommand prevNetCommand { get; set; }

    public bool GetHeld(Input input)
    {
        if (!puppet || NetCommand == null)
            return InputManager.GetHeld(input);

        return NetCommand.inputs[(int)input];
    }

    public bool GetPressed(Input input)
    {
        if (!puppet || NetCommand == null || prevNetCommand == null)
            return InputManager.GetPressed(input);
        return NetCommand.inputs[(int)input] && !prevNetCommand.inputs[(int)input];
    }

    public bool GetReleased(Input input)
    {
        if (!puppet || NetCommand == null || prevNetCommand == null)
            return InputManager.GetReleased(input);
        return !NetCommand.inputs[(int)input] && prevNetCommand.inputs[(int)input];
    }

}