using AM2E.Control;

namespace AM2E.Networking;

internal class NetCommand
{
    public bool[] inputs = new bool[NetworkGeneral.MAXINPUTS];

    internal NetCommand(bool empty = true)
    {
        if (!empty)
        {
            var i = 0;
            foreach (var name in Enum.GetNames(InputManager.EnumType))
            {
                inputs[i] = InputManager.GetHeld(name);
                i++;
            }
        }
        else
        {
            foreach (int i in Enum.GetValues(InputManager.EnumType))
            {
                inputs[i] = false;
            }
        }
    }

    internal static void SerializeWithCurrentInput(BitPackedData data, int tick)
    {
        data.WriteBits(tick, 10);
            
        foreach (var i in Enum.GetNames(InputManager.EnumType))
        {
            data.WriteBool(InputManager.GetHeld(i));
        }
    }

    internal void Serialize(BitPackedData data)
    {
        foreach (int i in Enum.GetValues(InputManager.EnumType))
        {
            data.WriteBool(inputs[i]);
        }
    }

    internal void Deserialize(BitPackedData data) 
    { 
        foreach (int i in Enum.GetValues(InputManager.EnumType))
        {
            inputs[i] = data.ReadBool();
        }
    }
}