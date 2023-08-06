using GameContent.EngineConfig;
using System;
using AM2E.Control;

namespace AM2E.Networking;

internal class NetCommand
{
    public bool[] inputs = new bool[NetworkGeneral.MAXINPUTS];

    internal NetCommand(bool empty = true)
    {
        if (!empty)
        {
            foreach (Input i in Enum.GetValues(typeof(Input)))
            {
                inputs[(int)i] = InputManager.GetHeld(i);
            }
        }
        else
        {
            foreach (int i in Enum.GetValues(typeof(Input)))
            {
                inputs[i] = false;
            }
        }
    }

    internal static void SerializeWithCurrentInput(BitPackedData data, int tick)
    {
        data.WriteBits(tick, 10);
            
        foreach (Input i in Enum.GetValues(typeof(Input)))
        {
            data.WriteBool(InputManager.GetHeld(i));
        }
    }

    internal void Serialize(BitPackedData data)
    {
        foreach (int i in Enum.GetValues(typeof(Input)))
        {
            data.WriteBool(inputs[i]);
        }
    }

    internal void Deserialize(BitPackedData data) 
    { 
        foreach (int i in Enum.GetValues(typeof(Input)))
        {
            inputs[i] = data.ReadBool();
        }
    }
}