
namespace AM2E.Networking;

internal class ControllableCreationEvent: NetReliableData
{
    internal string Type;
    internal string ID;
    internal string Layer;
    internal int X;
    internal int Y;
    internal int Master;

    internal ControllableCreationEvent(string type = "", string iD = "", string layer = "", int x = 0, int y = 0, int master = 0)
    {
        Type = type;
        ID = iD;
        Layer = layer;
        X = x;
        Y = y;
        Master = master;
    }

    internal override void Serialize(BitPackedData data)
    {
        // Reliable Message type 2.
        data.WriteBits(2, 8);
        data.WriteID(ID);
        data.WriteString(Type);
        data.WriteString(Layer);
        data.WriteBits(X, 16);
        data.WriteBits(Y, 16);
        data.WriteBits(Master, 8);
    }

    internal override void Deserialize(BitPackedData data)
    {
        ID = data.ReadID();
        Type = data.ReadString(50);
        Layer = data.ReadString(50);
        X = data.ReadBits(16);
        Y = data.ReadBits(16);
        Master = data.ReadBits(8);
    }
}