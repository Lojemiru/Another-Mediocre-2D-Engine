using AM2E.Actors;
using ENet;
using static AM2E.Networking.NetworkHelpers;

namespace AM2E.Networking;

public static class NetworkManager
{
    private static bool isNetworking;

    internal static bool isConnected;

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

    public static int RemotePeerId { get; internal set; } = -1;

    public static event Action<int>? PeerConnected;
    
    public static event Action<int>? PeerDisconnected;

    public static event Action<int>? ConnectedToServer;

    public static event Action? DisconnectedFromServer;

    const byte ServerPeerId = 0;

    static Host? host;

    internal readonly static Dictionary<uint, Peer> connectedPeers = [];

    readonly static Dictionary<int, StaticNetworkedActor> staticNetworkedActors = [];

    const int DEFAULT_MAX_CLIENTS = 32;

    const int NUM_CHANNELS = 5;

    internal static void OnPeerConnected(int peerId)
    {
        PeerConnected?.Invoke(peerId);
    }

    internal static void OnPeerDisconnected(int peerId)
    {
        PeerDisconnected?.Invoke(peerId);
    }

    internal static void OnConnectedToServer(int remotePeerId)
    {
        ConnectedToServer?.Invoke(remotePeerId);
    }

    public static void StartServer(int port, int maxClients = DEFAULT_MAX_CLIENTS)
    {
        try
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
        catch (Exception)
        {
            StopServer();
            throw;
        }
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
        try
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
        catch (Exception)
        {
            StopClient();
            throw;
        }
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
        if (isConnected)
        {
            DisconnectedFromServer?.Invoke();
        }
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

        // Host should be non-null when isNetworking is true
        if (isServer)
        {
            Server.ServerTick(host!);
        }
        else
        {
            Client.ClientTick(host!);
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

    internal static void RegisterStaticActor(int networkId, StaticNetworkedActor actor)
    {
        staticNetworkedActors.Add(networkId, actor);
    }

    internal static void UnregisterStaticActor(int networkId)
    {
        staticNetworkedActors.Remove(networkId);
    }

    public static void KickPeer(int peerId)
    {
        if (!IsServer)
        {
            throw new InvalidOperationException("Cannot kick players when not running an active server");
        }
        Server.KickPeer(peerId);
    }

    public static string GetPeerIP(int peerId)
    {
        if (!IsServer)
        {
            throw new InvalidOperationException("Cannot get ip when not running an active server");
        }
        if (connectedPeers.TryGetValue((uint)peerId, out var peer))
        {
            return peer.IP;
        }
        return "";
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

    private static void ClientSendDataPacket(MemoryStream packetStream, byte[] data, PacketReliability reliability, int channelId)
    {
        packetStream.Write(data);
        var packet = CreateENetPacket(packetStream.ToArray(), reliability);
        var enetChannel = GetENetChannelId(reliability, channelId);
        host!.Broadcast(enetChannel, ref packet);
    }

    private static void ServerSendDataPacket(MemoryStream packetStream, byte[] data, PacketReliability reliability, int channelId, List<int> targetPeers)
    {
        packetStream.Write(data);
        var packet = CreateENetPacket(packetStream.ToArray(), reliability);

        var enetChannel = GetENetChannelId(reliability, channelId);
        
        if (targetPeers.Count > 0)
        {
            var peers = targetPeers.Select(x => connectedPeers.GetValueOrDefault((uint)x)).ToArray();
            host!.Broadcast(enetChannel, ref packet, peers);
        }
        else
        {
            host!.Broadcast(enetChannel, ref packet);
        }
    }

    internal static void HandleDataPacket(Guid id, byte[] data, int senderId)
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

    internal static void HandleStaticDataPacket(int networkId, byte[] data, int senderId)
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

    internal static void DisconnectPeer(uint peerId)
    {
        if (IsServer && connectedPeers.TryGetValue(peerId, out var peer))
        {
            NotifyPeersOfDisconnection(peer);
            OnPeerDisconnected((int)peerId);
        }
        connectedPeers.Remove(peerId);
    }

    private static void NotifyPeersOfDisconnection(Peer peer)
    {
        var peerId = peer.ID + 1;
        var defaultReliableChannel = GetENetChannelId(PacketReliability.Reliable, 0);
        var disconnectData = new byte[2];
        disconnectData[0] = (byte)PacketTypes.Disconnect;
        disconnectData[1] = (byte)peerId;
        var disconnectPacket = CreateENetPacket(disconnectData, PacketReliability.Reliable);

        host!.Broadcast(defaultReliableChannel, ref disconnectPacket);
    }
}
