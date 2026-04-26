using ENet;
using static AM2E.Networking.NetworkManager;
using static AM2E.Networking.NetworkHelpers;

namespace AM2E.Networking;

internal static class Client
{
    internal static Action<bool> finishedAction = (_) => {};

    private static void OnConnectionFinished(bool wasSuccessful)
    {
        finishedAction(wasSuccessful);
        finishedAction = (_) => {};
    }

    private static void ParseDataPacket(MemoryStream packetStream, int senderId)
    {
        var guidBytes = new byte[16];
        try
        {
            packetStream.ReadExactly(guidBytes);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error when reading data packet:\n{ex}");
            return;
        }

        var guid = new Guid(guidBytes);
        var data = new byte[packetStream.Length - packetStream.Position];
        packetStream.ReadExactly(data);
        HandleDataPacket(guid, data, senderId);
    }

    private static void ParseStaticDataPacket(MemoryStream packetStream, int senderId)
    {
        if (packetStream.Length - packetStream.Position < 4)
        {
            Logger.Warn($"Error when reading data packet: Packet is not long enough!");
            return;
        }
        using var br = GetBinaryReader(packetStream);

        var networkId = br.ReadInt32();
        var data = new byte[packetStream.Length - packetStream.Position];
        packetStream.ReadExactly(data);
        HandleStaticDataPacket(networkId, data, senderId);
    }

    private static void ClientHandlePacket(Packet packet)
    {
        if (packet.Length == 0)
        {
            Logger.Warn("Received malformed packet");
            return;
        }

        var bytes = new byte[packet.Length];
        packet.CopyTo(bytes);

        using var ms = new MemoryStream(bytes);
        var packetType = (PacketTypes)ms.ReadByte();

        switch (packetType)
        {
            case PacketTypes.Data:
                {
                    var senderId = ms.ReadByte();
                    if (senderId == -1)
                    {
                        Logger.Warn($"Error: Malformed data packet");
                    }

                    var idType = (IdTypes)ms.ReadByte();
                    switch (idType)
                    {
                        case IdTypes.Guid:
                            ParseDataPacket(ms, senderId);
                            break;
                        case IdTypes.Static:
                            ParseStaticDataPacket(ms, senderId);
                            break;
                        default:
                            Logger.Warn($"Unexpected id type in packet body");
                            break;
                    }
                    break;
                }
            case PacketTypes.Disconnect:
                {
                    var disconnectedPeerId = ms.ReadByte();
                    if (disconnectedPeerId == -1)
                    {
                        Logger.Warn("Error: Malformed disconnect packet");
                        return;
                    }
                    Logger.Debug($"Client disconnected with ID: {disconnectedPeerId}");
                    OnPeerDisconnected(disconnectedPeerId);
                    break;
                }
            case PacketTypes.Connect:
                {
                    var connectedPeerId = ms.ReadByte();
                    if (connectedPeerId == -1)
                    {
                        Logger.Warn($"Error: Malformed connect packet");
                        return;
                    }
                    OnPeerConnected(connectedPeerId);
                    Logger.Debug($"Client connected with ID: {connectedPeerId}");
                    break;
                }
            case PacketTypes.ConnectionEstablished:
                {
                    var remotePeerId = ms.ReadByte();
                    if (remotePeerId == -1)
                    {
                        Logger.Warn($"Error: Malformed connection established packt");
                        return;
                    }
                    Logger.Debug($"Finished connecting to server, remote peer id: {remotePeerId}");
                    isConnected = true;
                    RemotePeerId = remotePeerId;
                    OnConnectedToServer(remotePeerId);
                    OnConnectionFinished(true);
                    break;
                }
            default:
                {
                    Logger.Warn($"Received unrecognised packet type");
                    break;
                }
        }
    }

    internal static void ClientTick(Host host)
    {
        for (var result = host.Service(0, out var netEvent); result > 0; result = host.CheckEvents(out netEvent))
        {
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    connectedPeers.Add(netEvent.Peer.ID, netEvent.Peer);
                    Logger.Debug($"Server acknowledged connection, waiting for full connection to be established");
                    break;
                case EventType.Disconnect:
                    connectedPeers.Remove(netEvent.Peer.ID);
                    Logger.Debug("Server disconnected");
                    if (!isConnected)
                    {
                        OnConnectionFinished(false);
                    }
                    StopClient();
                    return;
                case EventType.Receive:
                    {
                        using var packet = netEvent.Packet;
                        ClientHandlePacket(packet);
                        break;
                    }
                case EventType.Timeout:
                    connectedPeers.Remove(netEvent.Peer.ID);
                    Logger.Debug("Server timed out");
                    if (!isConnected)
                    {
                        OnConnectionFinished(false);
                    }
                    StopClient();
                    return;
            }
        }	
    }
}
