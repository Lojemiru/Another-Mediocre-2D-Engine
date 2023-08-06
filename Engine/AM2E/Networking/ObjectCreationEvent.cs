
namespace AM2E.Networking;

internal class ObjectCreationEvent: NetReliableData
{
    internal string Type;
    internal string ID;
    internal string Layer;
    internal int X;
    internal int Y;

    internal ObjectCreationEvent(string type = "", string iD = "", string layer = "", int x = 0, int y = 0)
    {
        Type = type;
        ID = iD;
        Layer = layer;
        X = x;
        Y = y;
    }

    internal override void Deserialize(BitPackedData data)
    {
        ID = data.ReadID();
        Type = data.ReadString(50);
        Layer = data.ReadString(50);
        X = data.ReadBits(16);
        Y = data.ReadBits(16);
    }

    internal override void Serialize(BitPackedData data)
    {
        // Reliable Message type 1.
        data.WriteBits(1, 8);
        data.WriteID(ID);
        data.WriteString(Type);
        data.WriteString(Layer);
        data.WriteBits(X, 16);
        data.WriteBits(Y, 16);
    }
}