using LiteNetLib;

namespace AM2E.Networking;
internal class NetworkedEntityData
{
    internal string Type { get; set; }
    internal int X { get; set; }
    internal int Y { get; set; }
    internal string Layer { get; set; }
    internal string ID { get; set; }
    internal INetSynced Instance { get; set; }

    internal readonly Dictionary<NetPeer, int> UnacknowledgedCreate = new();
    internal readonly Dictionary<NetPeer, int> UnacknowledgedDestroy = new();

    internal void SerializeCreate(BitPackedData data)
    {
        // Reliable Message type 1.
        data.WriteBits(1, 8);
        data.WriteID(ID);
        data.WriteString(Type);
        data.WriteString(Layer);
        data.WriteBits(X, 16);
        data.WriteBits(Y, 16);
    }

    internal void SerializeDestroy(BitPackedData data)
    {
        // Reliable Message type 2
        data.WriteBits(2, 8);
        data.WriteID(ID);
    }

    internal void DeserializeCreate(BitPackedData data)
    {
        ID = data.ReadID();
        Type = data.ReadString(50);
        Layer = data.ReadString(50);
        X = data.ReadBits(16);
        Y = data.ReadBits(16);
    }
}

