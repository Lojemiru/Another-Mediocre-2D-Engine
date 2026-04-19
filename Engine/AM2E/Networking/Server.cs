using ENet;
using static AM2E.Networking.NetworkManager;

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
        var connectionEstablishedData = new byte[2];
        connectionEstablishedData[0] = (byte)PacketTypes.ConnectionEstablished;
        connectionEstablishedData[1] = (byte)peerId;
        var connectionEstablishedPacket = default(Packet);
        connectionEstablishedPacket.Create(connectionEstablishedData, PacketFlags.Reliable);

        var connectionData = new byte[2];
        connectionData[0] = (byte)PacketTypes.Connect;
        connectionData[1] = (byte)peerId;
        var connectionDataPacket = default(Packet);
        connectionDataPacket.Create(connectionData, PacketFlags.Reliable);

        host.Broadcast(1, ref connectionDataPacket, peer);
        peer.Send(1, ref connectionEstablishedPacket);

        foreach (var peerKey in connectedPeers.Keys)
        {
            var existingPeerData = new byte[2];
            existingPeerData[0] = (byte)PacketTypes.Connect;
            existingPeerData[1] = (byte)peerKey;
            var existingPeerPacket = default(Packet);
            existingPeerPacket.Create(existingPeerData, PacketFlags.Reliable);
            peer.Send(1, ref existingPeerPacket);
        }
    }

    private static void NotifyPeersOfDisconnection(Host host, Peer peer)
    {
        var peerId = peer.ID + 1;
        var disconnectData = new byte[2];
        disconnectData[0] = (byte)PacketTypes.Disconnect;
        disconnectData[1] = (byte)peerId;
        var disconnectPacket = default(Packet);
        disconnectPacket.Create(disconnectData, PacketFlags.Reliable);

        host.Broadcast(1, ref disconnectPacket);
    }

    private static (List<Peer>?, bool) TryGetTargetPeers(Stream packetStream)
    {
        try
        {
            var peerCount = packetStream.ReadByte();
            var packetIsForServer = false;
            var peers = new List<Peer>();
            for (var i = 0; i < peerCount; i++)
            {
                var peer = packetStream.ReadByte();
                if (peer == ServerPeerId)
                {
                    packetIsForServer = true;
                }
                else
                {
                    peers.Add(connectedPeers.GetValueOrDefault((uint)peer));
                }
            }
            return (peers, packetIsForServer);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error when parsing packet header:\n{ex}");
            return (null, false);
        }
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
        var rebroadcastPacket = default(Packet);
        var reliability = (PacketReliability)(channelId % 3);
        var flags = ReliabilityToPacketFlags(reliability);
        rebroadcastPacket.Create(rebroadcastData, flags);

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
                        connectedPeers.Remove(peerId);
                        NotifyPeersOfDisconnection(host, netEvent.Peer);
                        OnPeerDisconnected((int)peerId);
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
                        connectedPeers.Remove(peerId);
                        Logger.Debug($"Peer timed out with ID: {peerId}");
                        NotifyPeersOfDisconnection(host, netEvent.Peer);
                        OnPeerDisconnected((int)peerId);
                        break;
                    }
                    
            }
        }
    }
}
