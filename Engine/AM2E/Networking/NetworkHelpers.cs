using System.Text;
using ENet;

namespace AM2E.Networking;

internal static class NetworkHelpers
{
    internal enum PacketTypes
    {
        Data = 0,
        Connect = 1,
        Disconnect = 2,
        ConnectionEstablished = 3
    }

    internal enum IdTypes
    {
        Guid = 0,
        Static = 1
    }

    internal static PacketFlags ReliabilityToPacketFlags(PacketReliability reliability)
    {
        return reliability switch
        {
            PacketReliability.Reliable => PacketFlags.Reliable,
            PacketReliability.UnreliableOrdered => PacketFlags.UnreliableFragmented,
            PacketReliability.Unreliable => PacketFlags.UnreliableFragmented | PacketFlags.Unsequenced,
            _ => throw new Exception("Invalid reliability value")
        };
    }

    internal static Packet CreateENetPacket(byte[] data, PacketReliability reliability)
    {
        var packet = default(Packet);
        packet.Create(data, ReliabilityToPacketFlags(reliability));
        return packet;
    }

    // This would actually be a really good situation for strong typing
    internal static byte GetENetChannelId(PacketReliability reliability, int channelId)
    {
        return (byte)(reliability + channelId * 3);        
    }

    // We don't want our streams closed randomly
    internal static BinaryReader GetBinaryReader(Stream stream)
    {
        return new BinaryReader(stream, Encoding.UTF8, true);
    }

    internal static BinaryWriter GetBinaryWriter(Stream stream)
    {
        return new BinaryWriter(stream, Encoding.UTF8, true);
    }
}
