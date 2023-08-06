using LiteNetLib;
using System.Collections.Generic;

namespace AM2E.Networking;

internal abstract class NetReliableData
{
    internal readonly Dictionary<NetPeer, int> SentTicks = new();
    internal abstract void Serialize(BitPackedData data);
    internal abstract void Deserialize(BitPackedData data);
}