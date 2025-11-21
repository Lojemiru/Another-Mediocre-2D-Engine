using AM2E.Actors;
using ENet;

namespace AM2E.Networking;

public static class NetworkManager
{
	public static bool IsNetworking { get; private set; }

	public static bool IsConnected { get; private set; }

	public static bool IsServer { get; private set; }

	public static event Action<int>? PeerConnected;
	
	public static event Action<int>? PeerDisconnected;

	public static event Action<int>? ConnectedToServer;

	const byte ServerPeerId = 255;

	enum PacketTypes
	{
		Data = 0,
		Connect = 1,
		Disconnect = 2,
		ConnectionEstablished = 3
	}

	static Host? host;

	readonly static Dictionary<uint, Peer> connectedPeers = [];

	const int DEFAULT_MAX_CLIENTS = 32;

	public static void StartServer(int port, int maxClients = DEFAULT_MAX_CLIENTS)
	{
		if (!IsNetworking)
		{
			InitializeNetworking();
		}
		DisposeHost();
		
		host = new Host();
		IsServer = true;
		IsConnected = true;
		var address = new Address() { Port = (ushort)port };
		host.Create(address, maxClients, 2);
		Logger.Debug("Started server");
	}

	public static void StopServer()
	{
		if (!IsNetworking)
		{
			throw new InvalidOperationException("Tried to stop server that wasn't running");
		}
		if (!IsServer)
		{
			throw new InvalidOperationException("Tried to stop client through StopServer method");
		}
		StopNetworking();
	}

	public static void StartClient(string ip, int port)
	{
		if (!IsNetworking)
		{
			InitializeNetworking();
		}
		DisposeHost();

		host = new Host();
		IsServer = false;
		
		var address = new Address() { Port = (ushort)port };
		address.SetIP(ip);

		host.Create();

		host.Connect(address, 2);
		Logger.Debug("Started client");
	}

	public static void StopClient()
	{
		if (!IsNetworking)
		{
			throw new InvalidOperationException("Tried to stop client that wasn't running");
		}
		if (IsServer)
		{
			throw new InvalidOperationException("Tried to stop server through StopClient method");
		}
		StopNetworking();
	}

	private static void InitializeNetworking()
	{
		Library.Initialize();
		IsNetworking = true;
	}

	private static void DisposeHost()
	{
		IsConnected = false;
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
		IsNetworking = false;
		DisposeHost();
		Library.Deinitialize();
	}

	public static void NetworkTick()
	{
		if (!IsNetworking)
		{
			return;
		}

		if (IsServer)
		{
			ServerTick();
		}
		else
		{
			ClientTick();
		}
	}

	public static void SendPacketToRemoteActor(Guid actorId, byte[] data, bool isReliable, List<int>? targetPeers = null)
	{
		if (!IsNetworking || !IsConnected)
		{
			return;
		}

		targetPeers ??= [];

		Logger.Debug($"Beginning packet send to {actorId}");

		if (IsServer)
		{
			ServerSendDataPacket(actorId, data, isReliable, targetPeers);
		}
		else
		{
			ClientSendDataPacket(actorId, data, isReliable, targetPeers);
		}
	}

	// Packet structure Client -> Server:
	// PeerCount, 0 for broadcast
	// Peers
	// actorId
	// data
	private static void ClientSendDataPacket(Guid actorId, byte[] data, bool isReliable, List<int> targetPeers)
	{
		using var ms = new MemoryStream();
		var guidBytes = actorId.ToByteArray();
		ms.WriteByte((byte)targetPeers.Count);
		foreach (var peerId in targetPeers)
		{
			var peerByte = peerId == -1 ? ServerPeerId : (byte)peerId;
			ms.WriteByte(peerByte);
		}
		ms.Write(guidBytes);
		ms.Write(data);
		var packet = default(Packet);
		var flags = isReliable ? PacketFlags.Reliable : PacketFlags.None;
		packet.Create(ms.ToArray(), flags);
		var channelId = isReliable ? 1 : 0;
		host!.Broadcast((byte)channelId, ref packet);
	}

	// Packet structure Server -> Client:
	// PacketType
	// SenderId
	// actorId
	// data
	private static void ServerSendDataPacket(Guid actorId, byte[] data, bool isReliable, List<int> targetPeers)
	{
		using var ms = new MemoryStream();
		var guidBytes = actorId.ToByteArray();
		ms.WriteByte((byte)PacketTypes.Data);
		ms.WriteByte(ServerPeerId);
		ms.Write(guidBytes);
		ms.Write(data);
		var packet = default(Packet);
		var flags = isReliable ? PacketFlags.Reliable : PacketFlags.None;
		packet.Create(ms.ToArray(), flags);

		var channelId = isReliable ? 1 : 0;
		
		if (targetPeers.Count > 0)
		{
			var peers = targetPeers.Select(x => connectedPeers.GetValueOrDefault((uint)x)).ToArray();
			host!.Broadcast((byte)channelId, ref packet, peers);
		}
		else
		{
			host!.Broadcast((byte)channelId, ref packet);
		}
	}

	private static void HandleDataPacket(Guid id, byte[] data, int senderId)
	{
		for (var i = 0; i < data.Length; i++)
		{
			Logger.Debug($"Packet data i: {data[i]}");
		}
		var actor = Actor.GetActor(id.ToString()) as INetworkedActor;
		if (actor is not null)
		{
			actor.OnPacketReceive(data, senderId);
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

	private static void ServerHandlePacket(Packet packet, int peerId, byte channelId)
	{
		var bytes = new byte[packet.Length];
		packet.CopyTo(bytes);

		using var ms = new MemoryStream(bytes);
		var peerCount = ms.ReadByte();
		var isBroadcast = peerCount == 0;
		var packetIsForServer = isBroadcast;
		var rebroadcastPeers = new List<Peer>();
		var sendingPeer = connectedPeers.GetValueOrDefault((uint)peerId);
		for (var i = 0; i < peerCount; i++)
		{
			var peer = ms.ReadByte();
			if (peer == ServerPeerId)
			{
				packetIsForServer = true;
			}
			else
			{
				rebroadcastPeers.Add(connectedPeers.GetValueOrDefault((uint)peer));
			}
		}

		var data = new byte[packet.Length - ms.Position];
		ms.ReadExactly(data);
		var rebroadcastData = CreateRebroadcastData(data, peerId);
		var rebroadcastPacket = default(Packet);
		Logger.Debug($"Packet is reliable: {channelId == 1}");
		var flags = channelId == 1 ? PacketFlags.Reliable : PacketFlags.None;
		rebroadcastPacket.Create(rebroadcastData, flags);

		
		if (isBroadcast)
		{
			host!.Broadcast(channelId, ref rebroadcastPacket, sendingPeer);
		}
		else if (rebroadcastPeers.Count > 0)
		{
			host!.Broadcast(channelId, ref rebroadcastPacket, rebroadcastPeers.ToArray());
		}

		if (packetIsForServer)
		{
			var guidBytes = data.Take(16).ToArray();
			var guid = new Guid(guidBytes);
			HandleDataPacket(guid, data.Skip(16).ToArray(), peerId);
		}
	}

	private static void ClientHandlePacket(Packet packet)
	{
		var bytes = new byte[packet.Length];
		packet.CopyTo(bytes);

		using var ms = new MemoryStream(bytes);
		var packetType = (PacketTypes)ms.ReadByte();

		switch (packetType)
		{
			case PacketTypes.Data:
				{
					var senderId = ms.ReadByte();
					if (senderId == ServerPeerId)
					{
						senderId = -1;
					}

					var guidBytes = new byte[16];
					ms.ReadExactly(guidBytes);
					var guid = new Guid(guidBytes);

					var data = new byte[packet.Length - ms.Position];
					ms.ReadExactly(data);

					HandleDataPacket(guid, data, senderId);
					break;
				}
			case PacketTypes.Disconnect:
				{
					var disconnectedPeerId = ms.ReadByte();
					PeerDisconnected?.Invoke(disconnectedPeerId);
					Logger.Debug($"Client disconnected with ID: {disconnectedPeerId}");
					break;
				}
			case PacketTypes.Connect:
				{
					var connectedPeerId = ms.ReadByte();
					PeerConnected?.Invoke(connectedPeerId);
					Logger.Debug($"Client connected with ID: {connectedPeerId}");
					break;
				}
			case PacketTypes.ConnectionEstablished:
				{
					var remotePeerId = ms.ReadByte();
					ConnectedToServer?.Invoke(remotePeerId);
					Logger.Debug($"Finished connecting to server, remote peer id: {remotePeerId}");
					IsConnected = true;
					break;
				}
		}
	}

	// Notify newly connected peer of their remote peer id
	// Notify other peers of newly connected peer
	// Notify newly connected peer of all other peers
	private static void NotifyPeersOfConnection(Peer peer)
	{
		var connectionEstablishedData = new byte[2];
		connectionEstablishedData[0] = (byte)PacketTypes.ConnectionEstablished;
		connectionEstablishedData[1] = (byte)peer.ID;
		var connectionEstablishedPacket = default(Packet);
		connectionEstablishedPacket.Create(connectionEstablishedData, PacketFlags.Reliable);

		var connectionData = new byte[2];
		connectionData[0] = (byte)PacketTypes.Connect;
		connectionData[1] = (byte)peer.ID;
		var connectionDataPacket = default(Packet);
		connectionDataPacket.Create(connectionData, PacketFlags.Reliable);

		host!.Broadcast(1, ref connectionDataPacket, peer);
		peer.Send(1, ref connectionEstablishedPacket);

		foreach (var peerId in connectedPeers.Keys)
		{
			var existingPeerData = new byte[2];
			existingPeerData[0] = (byte)PacketTypes.Connect;
			existingPeerData[1] = (byte)peerId;
			var existingPeerPacket = default(Packet);
			existingPeerPacket.Create(existingPeerData, PacketFlags.Reliable);
			peer.Send(1, ref existingPeerPacket);
		}
	}

	private static void NotifyPeersOfDisconnection(Peer peer)
	{
		var disconnectData = new byte[2];
		disconnectData[0] = (byte)PacketTypes.Disconnect;
		disconnectData[1] = (byte)peer.ID;
		var disconnectPacket = default(Packet);
		disconnectPacket.Create(disconnectData, PacketFlags.Reliable);

		host!.Broadcast(1, ref disconnectPacket);
	}

	private static void ServerTick()
	{
		while (host!.Service(0, out var netEvent) > 0)
		{
			switch (netEvent.Type)
			{
				case EventType.Connect:
					NotifyPeersOfConnection(netEvent.Peer);
					connectedPeers.Add(netEvent.Peer.ID, netEvent.Peer);
					PeerConnected?.Invoke((int)netEvent.Peer.ID);
					Logger.Debug($"Peer connected with ID: {netEvent.Peer.ID}");
					break;
				case EventType.Disconnect:
					connectedPeers.Remove(netEvent.Peer.ID);
					NotifyPeersOfDisconnection(netEvent.Peer);
					PeerDisconnected?.Invoke((int)netEvent.Peer.ID);
					Logger.Debug($"Peer disconnected with ID: {netEvent.Peer.ID}");
					break;
				case EventType.Receive:
					{
						Logger.Debug("Packet received");
						using var packet = netEvent.Packet;
						ServerHandlePacket(packet, (int)netEvent.Peer.ID, netEvent.ChannelID);
						break;
					}
				case EventType.Timeout:
					connectedPeers.Remove(netEvent.Peer.ID);
					Logger.Debug($"Peer timed out with ID: {netEvent.Peer.ID}");
					NotifyPeersOfDisconnection(netEvent.Peer);
					PeerDisconnected?.Invoke((int)netEvent.Peer.ID);
					break;
			}
		}
	}

	private static void ClientTick()
	{
		while (host!.Service(0, out var netEvent) > 0)
		{
			switch (netEvent.Type)
			{
				case EventType.Connect:
					connectedPeers.Add(netEvent.Peer.ID, netEvent.Peer);
					Logger.Debug($"Connected to server");
					break;
				case EventType.Disconnect:
					connectedPeers.Remove(netEvent.Peer.ID);
					Logger.Debug("Server disconnected");
					break;
				case EventType.Receive:
					{
						Logger.Debug("Packet received");
						using var packet = netEvent.Packet;
						ClientHandlePacket(packet);
						break;
					}
				case EventType.Timeout:
					connectedPeers.Remove(netEvent.Peer.ID);
					Logger.Debug("Server timed out");
					break;
			}
		}
	}
}
