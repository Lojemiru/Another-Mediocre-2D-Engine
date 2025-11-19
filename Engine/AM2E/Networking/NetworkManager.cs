using AM2E.Actors;
using ENet;

namespace AM2E.Networking;

public static class NetworkManager
{
	static bool isNetworking;
	static bool isServer;
	static Host? host;
	static Peer peer;
	static Dictionary<uint, Peer> connectedPeers = [];

	const int MAX_CLIENTS = 32;

	public static void StartServer(int port)
	{
		Library.Initialize();
		host = new Host();
		isServer = true;
		isNetworking = true;
		var address = new Address() { Port = (ushort)port };
		host.Create(address, MAX_CLIENTS);
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
	}

	public static void StartClient(string ip, int port)
	{
		Library.Initialize();
		host = new Host();
		isServer = false;
		isNetworking = true;
		
		var address = new Address() { Port = (ushort)port };
		address.SetIP(ip);

		host.Create();

		host.Connect(address);
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
		StopNetworking();
	}

	private static void StopNetworking()
	{
		isNetworking = false;
		host?.Dispose();
		host = null;
		Library.Deinitialize();
	}

	public static void NetworkTick()
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

	public static void SendPacketToRemoteActor(Guid actorId, byte[] data)
	{
		if (!isNetworking)
		{
			return;
		}

		Logger.Debug($"Beginning packet send to {actorId}");
		using var ms = new MemoryStream();
		var guidBytes = actorId.ToByteArray();
		ms.Write(guidBytes);
		ms.Write(data);
		var packet = default(Packet);
		packet.Create(ms.ToArray(), PacketFlags.Reliable);
		host!.Broadcast(0, ref packet);
	}

	static void HandlePacket(Packet packet)
	{
		var buffer = new byte[packet.Length];
		packet.CopyTo(buffer);
		using var ms = new MemoryStream(buffer);
		var guidBytes = new byte[16];
		ms.ReadExactly(guidBytes, 0, 16);
		var guid = new Guid(guidBytes);
		var data = new byte[packet.Length - 16];
		ms.ReadExactly(data);

		Logger.Debug($"Received guid: {guid}");
		var actor = Actor.GetActor(guid.ToString()) as INetworkedActor;
		if (actor is not null)
		{
			actor.OnPacketReceive(data);
		}
		

		packet.Dispose();
	}

	static void ServerTick()
	{
		while (host!.Service(0, out var netEvent) > 0)
		{
			switch (netEvent.Type)
			{
				case EventType.Connect:
					connectedPeers.Add(netEvent.Peer.ID, netEvent.Peer);
					Logger.Debug($"Peer connected with ID: {netEvent.Peer.ID}");
					break;
				case EventType.Disconnect:
					connectedPeers.Remove(netEvent.Peer.ID);
					Logger.Debug("Peer disconnected");
					break;
				case EventType.Receive:
					Logger.Debug("Packet received");
					HandlePacket(netEvent.Packet);
					break;
			}
		}
	}

	static void ClientTick()
	{
		while (host!.Service(0, out var netEvent) > 0)
		{
			switch (netEvent.Type)
			{
				case EventType.Connect:
					Logger.Debug("Peer connected");
					peer = netEvent.Peer;
					break;
				case EventType.Disconnect:
					Logger.Debug("Peer disconnected");
					break;
				case EventType.Receive:
					Logger.Debug("Packet received");
					HandlePacket(netEvent.Packet);
					break;
			}
		}
	}
}
