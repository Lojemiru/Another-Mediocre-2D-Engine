using ENet;
using static AM2E.Networking.NetworkManager;
using static AM2E.Networking.NetworkHelpers;

namespace AM2E.Networking;

internal static class Server
{
    const byte ServerPeerId = 0;

    // Notify newly connected peer of their remote peer id
    // Notify other peers of newly connected peer
    // Notify newly connected peer of all other peers
    private static void NotifyPeersOfConnection(Host host, Peer peer)
    {
        var peerId = peer.ID + 1;
        var defaultReliableChannel = GetENetChannelId(PacketReliability.Reliable, 0);
        var connectionEstablishedData = new byte[2];
        connectionEstablishedData[0] = (byte)PacketTypes.ConnectionEstablished;
        connectionEstablishedData[1] = (byte)peerId;
        var connectionEstablishedPacket = CreateENetPacket(connectionEstablishedData, PacketReliability.Reliable);

        var connectionData = new byte[2];
        connectionData[0] = (byte)PacketTypes.Connect;
        connectionData[1] = (byte)peerId;
        var connectionDataPacket = CreateENetPacket(connectionData, PacketReliability.Reliable);

        host.Broadcast(defaultReliableChannel, ref connectionDataPacket, peer);
        peer.Send(defaultReliableChannel, ref connectionEstablishedPacket);

        foreach (var peerKey in connectedPeers.Keys)
        {
            var existingPeerData = new byte[2];
            existingPeerData[0] = (byte)PacketTypes.Connect;
            existingPeerData[1] = (byte)peerKey;
            var existingPeerPacket = CreateENetPacket(existingPeerData, PacketReliability.Reliable);
            peer.Send(defaultReliableChannel, ref existingPeerPacket);
        }
    }

    private static (List<Peer>?, bool) TryGetTargetPeers(Stream packetStream)
    {
        var peerCount = packetStream.ReadByte();

        if (peerCount == -1)
        {
            Logger.Warn("Hit end of stream when parsing packet header peer count");
            return (null, false);
        }

        var peerBytes = new byte[peerCount];

        for (var i = 0; i < peerCount; i++)
        {
            var num = packetStream.ReadByte();
            if (num == -1)
            {
                Logger.Warn("Hit end of stream when parsing packet header peer count");
                return (null, false);
            }
            peerBytes[i] = (byte)num;
        }

        var packetIsForServer = false;
        var peers = new List<Peer>();
        foreach (var peer in peerBytes)
        {
            if (peer == ServerPeerId)
            {
                packetIsForServer = true;
            }
            else
            {
                peers.Add(connectedPeers.GetValueOrDefault(peer));
            }
        }
        return (peers, packetIsForServer);
    }

    private static byte[] CreateRebroadcastData(byte[] data, int peerId)
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte)PacketTypes.Data);
        ms.WriteByte((byte)peerId);
        ms.Write(data);

        return ms.ToArray();
    }

    private static void ServerHandleGuidPacket(MemoryStream dataStream, int senderId)
    {
        var guidBytes = new byte[16];
        try
        {
            dataStream.ReadExactly(guidBytes);
        }
        catch (EndOfStreamException)
        {
            Logger.Warn($"Error parsing Guid packet body, packet too short. Packet body length: {dataStream.Length}");
            return;
        }
        var guid = new Guid(guidBytes);
        var data = new byte[dataStream.Length - dataStream.Position];
        dataStream.ReadExactly(data);
        HandleDataPacket(guid, data, senderId);
    }

    private static void ServerHandleStaticPacket(MemoryStream dataStream, int senderId)
    {
        using var br = GetBinaryReader(dataStream);
        var networkId = 0;
        try
        {
            networkId = br.ReadInt32();
        }
        catch (EndOfStreamException)
        {
            Logger.Warn($"Error parsing Static packet body, packet too short. Packet body length: {dataStream.Length}");
            return;
        }
        var data = new byte[dataStream.Length - dataStream.Position];
        dataStream.ReadExactly(data);
        HandleStaticDataPacket(networkId, data, senderId);
    }

    private static void ServerHandlePacket(Host host, Packet packet, int peerId, byte channelId)
    {
        var bytes = new byte[packet.Length];
        packet.CopyTo(bytes);

        using var ms = new MemoryStream(bytes);
        var (rebroadcastPeers, packetIsForServer) = TryGetTargetPeers(ms);
        if (rebroadcastPeers is null)
        {
            Logger.Warn($"Packet of length: {packet.Length} failed to parse");
            return;
        }

        var sendingPeer = connectedPeers.GetValueOrDefault((uint)peerId);
        var isBroadcast = rebroadcastPeers.Count == 0 && !packetIsForServer;
        packetIsForServer = packetIsForServer || isBroadcast;

        var data = new byte[packet.Length - ms.Position];
        ms.ReadExactly(data);
        var rebroadcastData = CreateRebroadcastData(data, peerId);
        var reliability = (PacketReliability)(channelId % 3);
        var rebroadcastPacket = CreateENetPacket(rebroadcastData, reliability);

        if (isBroadcast)
        {
            host.Broadcast(channelId, ref rebroadcastPacket, excludedPeer: sendingPeer);
        }
        else if (rebroadcastPeers.Count > 0)
        {
            host.Broadcast(channelId, ref rebroadcastPacket, [.. rebroadcastPeers]);
        }

        if (packetIsForServer)
        {
            using var dataStream = new MemoryStream(data);
            var idType = (IdTypes)dataStream.ReadByte();
            switch (idType)
            {
                case IdTypes.Guid:
                    ServerHandleGuidPacket(dataStream, peerId);
                    break;
                case IdTypes.Static:
                    ServerHandleStaticPacket(dataStream, peerId);
                    break;
                default:
                    Logger.Warn("Unexpected id type in packet body");
                    break;
            }
        }
    }

    internal static void KickPeer(int peerId)
    {
        if (connectedPeers.TryGetValue((uint)peerId, out var peer))
        {
            peer.DisconnectNow(0);
            DisconnectPeer((uint)peerId);
        }
    }

    internal static void ServerTick(Host host)
    {
        for (var result = host.Service(0, out var netEvent); result > 0; result = host.CheckEvents(out netEvent))
        {
            var peerId = netEvent.Peer.ID + 1;
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    {
                        NotifyPeersOfConnection(host, netEvent.Peer);
                        connectedPeers.Add(peerId, netEvent.Peer);
                        OnPeerConnected((int)peerId);
                        Logger.Debug($"Peer connected with ID: {peerId}");
                        break;
                    }
                case EventType.Disconnect:
                    {
                        DisconnectPeer(peerId);
                        Logger.Debug($"Peer disconnected with ID: {peerId}");
                        break;
                    }
                case EventType.Receive:
                    {
                        using var packet = netEvent.Packet;
                        ServerHandlePacket(host, packet, (int)peerId, netEvent.ChannelID);
                        break;
                    }
                case EventType.Timeout:
                    {
                        DisconnectPeer(peerId);
                        Logger.Debug($"Peer timed out with ID: {peerId}");
                        break;
                    }
                    
            }
        }
    }
}
