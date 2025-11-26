using AM2E.Actors;

namespace AM2E.Networking;

public abstract class StaticNetworkedActor : Actor, INetworkedActor
{
	public int NetworkId = -1;

	public StaticNetworkedActor(int networkId)
		: base(0, 0, null!)
	{
		SetPersistent(true);
		NetworkId = networkId;
		NetworkManager.RegisterStaticActor(networkId, this);
	}

	public abstract void OnPacketReceive(byte[] data, int senderId);

	protected override void OnDispose()
	{
		base.OnDispose();
		NetworkManager.UnregisterStaticActor(NetworkId);
	}
}
