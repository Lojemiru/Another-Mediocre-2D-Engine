using AM2E.Actors;
using ENet;
using System.Text;

namespace AM2E.Networking;

public static class NetworkManager
{
    private static bool isNetworking;

    private static bool isConnected;

    private static bool isServer;

    public static bool IsMultiplayer
    {
        get
        {
            return isNetworking && isConnected;
        }
    }

    public static bool IsServer
    {
        get
        {
            return isServer && isConnected && isNetworking;
        }
    }

    public static bool IsClient
    {
        get
        {
            return !isServer && isConnected && isNetworking;
        }
    }

    public static int RemotePeerId { get; private set; } = -1;

    public static event Action<int>? PeerConnected;
    
    public static event Action<int>? PeerDisconnected;

    public static event Action<int>? ConnectedToServer;

    public static event Action? DisconnectedFromServer;

    const byte ServerPeerId = 0;

    enum PacketTypes
    {
        Data = 0,
        Connect = 1,
        Disconnect = 2,
        ConnectionEstablished = 3
    }

    enum IdTypes
    {
        Guid = 0,
        Static = 1
    }

    public enum PacketReliability
    {
        Reliable = 0,
        UnreliableOrdered = 1,
        Unreliable = 2
    }

    static Host? host;

    readonly static Dictionary<uint, Peer> connectedPeers = [];

    readonly static Dictionary<int, StaticNetworkedActor> staticNetworkedActors = [];

    const int DEFAULT_MAX_CLIENTS = 32;

    const int NUM_CHANNELS = 5;

    public static void StartServer(int port, int maxClients = DEFAULT_MAX_CLIENTS)
    {
        if (!isNetworking)
        {
            InitializeNetworking();
        }
        DisposeHost();
        
        host = new Host();
        isServer = true;
        isConnected = true;
        RemotePeerId = 0;
        var address = new Address() { Port = (ushort)port };
        host.Create(address, maxClients, NUM_CHANNELS * 3);
        Logger.Debug("Started server");
    }

    public static void StopServer()
    {
        if (!isNetworking)
        {
            throw new InvalidOperationException("Tried to stop server that wasn't running");
        }
        if (!isServer)
        {
            throw new InvalidOperationException("Tried to stop client through StopServer method");
        }
        StopNetworking();
        Logger.Debug("Stopped server");
    }

    public static void StartClient(string ip, int port)
    {
        if (!isNetworking)
        {
            InitializeNetworking();
        }
        DisposeHost();

        host = new Host();
        isServer = false;
        
        var address = new Address() { Port = (ushort)port };
        address.SetHost(ip);

        host.Create();

        host.Connect(address, NUM_CHANNELS * 3);
        Logger.Debug("Started client");
    }

    public static void StopClient()
    {
        if (!isNetworking)
        {
            throw new InvalidOperationException("Tried to stop client that wasn't running");
        }
        if (isServer)
        {
            throw new InvalidOperationException("Tried to stop server through StopClient method");
        }
        DisconnectedFromServer?.Invoke();
        StopNetworking();
        Logger.Debug("Stopped client");
    }

    private static void InitializeNetworking()
    {
        Library.Initialize();
        isNetworking = true;
    }

    private static void DisposeHost()
    {
        isConnected = false;
        foreach (var peer in connectedPeers.Values)
        {
            peer.Disconnect(0);
        }
        connectedPeers.Clear();
        host?.Flush();
        host?.Dispose();
        host = null;
    }

    private static void StopNetworking()
    {
        isNetworking = false;
        DisposeHost();
        Library.Deinitialize();
    }

    internal static void NetworkTick()
    {
        if (!isNetworking)
        {
            return;
        }

        if (isServer)
        {
            ServerTick();
        }
        else
        {
            ClientTick();
        }
    }

    internal static void NetworkFlush()
    {
        if (!isNetworking)
        {
            return;
        }

        host!.Flush();
    }

    // We don't want our streams closed randomly
    private static BinaryReader GetBinaryReader(Stream stream)
    {
        return new BinaryReader(stream, Encoding.UTF8, true);
    }

    private static BinaryWriter GetBinaryWriter(Stream stream)
    {
        return new BinaryWriter(stream, Encoding.UTF8, true);
    }

    internal static void RegisterStaticActor(int networkId, StaticNetworkedActor actor)
    {
        staticNetworkedActors.Add(networkId, actor);
    }

    internal static void UnregisterStaticActor(int networkId)
    {
        staticNetworkedActors.Remove(networkId);
    }

    public static void SendPacketToRemoteStaticActor(int networkId, byte[] data, PacketReliability reliability, List<int>? targetPeers = null, int channelId = 0)
    {
        targetPeers ??= [];
        if (!isNetworking || !isConnected || targetPeers.Count == 1 && targetPeers[0] == RemotePeerId)
        {
            return;
        }

        if (channelId < 0 || channelId >= NUM_CHANNELS)
        {
            throw new ArgumentException($"Channel id {channelId} was outside the bounds of allowed channel ids, Max: {NUM_CHANNELS - 1}, Min: 0");
        }

        using var ms = new MemoryStream();
        if (isServer)
        {
            WriteServerHeaderForStatic(ms, networkId);
            ServerSendDataPacket(ms, data, reliability, channelId, targetPeers);
        }
        else
        {
            WriteClientHeaderForStatic(ms, networkId, targetPeers);
            ClientSendDataPacket(ms, data, reliability, channelId);
        }
    }

    public static void SendPacketToRemoteActor(Guid actorId, byte[] data, PacketReliability reliability, List<int>? targetPeers = null, int channelId = 0)
    {
        targetPeers ??= [];
        if (!isNetworking || !isConnected || targetPeers.Count == 1 && targetPeers[0] == RemotePeerId)
        {
            return;
        }
        using var ms = new MemoryStream();
        if (isServer)
        {
            WriteServerHeaderForGuid(ms, actorId);
            ServerSendDataPacket(ms, data, reliability, channelId, targetPeers);
        }
        else
        {
            WriteClientHeaderForGuid(ms, actorId, targetPeers);
            ClientSendDataPacket(ms, data, reliability, channelId);
        }
    }

    private static void WriteClientHeaderForGuid(Stream packetStream, Guid actorId, List<int> targetPeers)
    {
        packetStream.WriteByte((byte)targetPeers.Count);
        foreach (var peerId in targetPeers)
        {
            packetStream.WriteByte((byte)peerId);
        }
        packetStream.WriteByte((byte)IdTypes.Guid);
        packetStream.Write(actorId.ToByteArray());
    }

    private static void WriteClientHeaderForStatic(Stream packetStream, int networkId, List<int> targetPeers)
    {
        packetStream.WriteByte((byte)targetPeers.Count);
        foreach (var peerId in targetPeers)
        {
            packetStream.WriteByte((byte)peerId);
        }
        packetStream.WriteByte((byte)IdTypes.Static);
        using var bw = GetBinaryWriter(packetStream);
        bw.Write(networkId);
        bw.Flush();
    }

    private static void WriteServerHeaderForStatic(Stream packetStream, int networkId)
    {
        packetStream.WriteByte((byte)PacketTypes.Data);
        packetStream.WriteByte(ServerPeerId);
        packetStream.WriteByte((byte)IdTypes.Static);
        using var bw = GetBinaryWriter(packetStream);
        bw.Write(networkId);
        bw.Flush();
    }

    private static void WriteServerHeaderForGuid(Stream packetStream, Guid actorId)
    {
        packetStream.WriteByte((byte)PacketTypes.Data);
        packetStream.WriteByte(ServerPeerId);
        packetStream.WriteByte((byte)IdTypes.Guid);
        packetStream.Write(actorId.ToByteArray());
    }

    private static PacketFlags ReliabilityToPacketFlags(PacketReliability reliability)
    {
        return reliability switch
        {
            PacketReliability.Reliable => PacketFlags.Reliable,
            PacketReliability.UnreliableOrdered => PacketFlags.UnreliableFragmented,
            PacketReliability.Unreliable => PacketFlags.UnreliableFragmented | PacketFlags.Unsequenced,
            _ => throw new Exception("Invalid reliability value")
        };
    }

    private static void ClientSendDataPacket(MemoryStream packetStream, byte[] data, PacketReliability reliability, int channelId)
    {
        packetStream.Write(data);
        var packet = default(Packet);
        var flags = ReliabilityToPacketFlags(reliability);
        packet.Create(packetStream.ToArray(), flags);
        var channel = reliability + channelId * 3;
        host!.Broadcast((byte)channel, ref packet);
    }

    private static void ServerSendDataPacket(MemoryStream packetStream, byte[] data, PacketReliability reliability, int channelId, List<int> targetPeers)
    {
        packetStream.Write(data);
        var packet = default(Packet);
        var flags = ReliabilityToPacketFlags(reliability);
        packet.Create(packetStream.ToArray(), flags);

        var channel = reliability + channelId * 3;
        
        if (targetPeers.Count > 0)
        {
            var peers = targetPeers.Select(x => connectedPeers.GetValueOrDefault((uint)x)).ToArray();
            host!.Broadcast((byte)channel, ref packet, peers);
        }
        else
        {
            host!.Broadcast((byte)channel, ref packet);
        }
    }

    private static void HandleDataPacket(Guid id, byte[] data, int senderId)
    {
        var actor = Actor.GetActor(id.ToString());
        if (actor is null || !(actor.Level?.Active ?? true))
        {
            return;
        }
        if (actor is INetworkedActor networkedActor)
        {
            networkedActor.OnPacketReceive(data, senderId);
        }
        else
        {
            Logger.Warn($"Received packet for id: {id} but actor with that id does not implement INetworkedActor");
        }
    }

    private static void HandleStaticDataPacket(int networkId, byte[] data, int senderId)
    {
        var actor = staticNetworkedActors.GetValueOrDefault(networkId);
        if (actor is not null)
        {
            actor.OnPacketReceive(data, senderId);
        }
        else
        {
            Logger.Debug($"Received packet for id: {networkId} but there was no local actor for that id");
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

    private static void ServerHandlePacket(Packet packet, int peerId, byte channelId)
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
            host!.Broadcast(channelId, ref rebroadcastPacket, excludedPeer: sendingPeer);
        }
        else if (rebroadcastPeers.Count > 0)
        {
            host!.Broadcast(channelId, ref rebroadcastPacket, rebroadcastPeers.ToArray());
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
                    PeerDisconnected?.Invoke(disconnectedPeerId);
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
                    PeerConnected?.Invoke(connectedPeerId);
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
                    ConnectedToServer?.Invoke(remotePeerId);
                    break;
                }
            default:
                {
                    Logger.Warn($"Received unrecognised packet type");
                    break;
                }
        }
    }

    // Notify newly connected peer of their remote peer id
    // Notify other peers of newly connected peer
    // Notify newly connected peer of all other peers
    private static void NotifyPeersOfConnection(Peer peer)
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

        host!.Broadcast(1, ref connectionDataPacket, peer);
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

    private static void NotifyPeersOfDisconnection(Peer peer)
    {
        var peerId = peer.ID + 1;
        var disconnectData = new byte[2];
        disconnectData[0] = (byte)PacketTypes.Disconnect;
        disconnectData[1] = (byte)peerId;
        var disconnectPacket = default(Packet);
        disconnectPacket.Create(disconnectData, PacketFlags.Reliable);

        host!.Broadcast(1, ref disconnectPacket);
    }

    private static void ServerTick()
    {
        for (var result = host!.Service(0, out var netEvent); result > 0; result = host.CheckEvents(out netEvent))
        {
            var peerId = netEvent.Peer.ID + 1;
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    {
                        NotifyPeersOfConnection(netEvent.Peer);
                        connectedPeers.Add(peerId, netEvent.Peer);
                        PeerConnected?.Invoke((int)peerId);
                        Logger.Debug($"Peer connected with ID: {peerId}");
                        break;
                    }
                case EventType.Disconnect:
                    {
                        connectedPeers.Remove(peerId);
                        NotifyPeersOfDisconnection(netEvent.Peer);
                        PeerDisconnected?.Invoke((int)peerId);
                        Logger.Debug($"Peer disconnected with ID: {peerId}");
                        break;
                    }
                case EventType.Receive:
                    {
                        using var packet = netEvent.Packet;
                        ServerHandlePacket(packet, (int)peerId, netEvent.ChannelID);
                        break;
                    }
                case EventType.Timeout:
                    {
                        connectedPeers.Remove(peerId);
                        Logger.Debug($"Peer timed out with ID: {peerId}");
                        NotifyPeersOfDisconnection(netEvent.Peer);
                        PeerDisconnected?.Invoke((int)peerId);
                        break;
                    }
                    
            }
        }
    }

    private static void ClientTick()
    {
        for (var result = host!.Service(0, out var netEvent); result > 0; result = host.CheckEvents(out netEvent))
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
                    StopClient();
                    return;
            }
        }	
    }
}
