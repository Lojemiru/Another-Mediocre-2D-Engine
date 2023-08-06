using GameContent.EngineConfig;
using AM2E.Control;

namespace AM2E.Networking;

public class Controller
{
    // TODO: Should we have a separate controller for this instead?
    public bool puppet = true;
        
    internal NetCommand NetCommand { get; set; }

    public bool GetHeld(Input input)
    {
        if (!puppet || NetCommand == null)
            return InputManager.GetHeld(input);

        return NetCommand.inputs[(int)input];
    }

}