namespace AM2E.Networking;

public interface INetworkedActor
{
	public void OnPacketReceive(byte[] data);
}
