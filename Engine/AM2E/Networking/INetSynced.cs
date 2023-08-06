
namespace AM2E.Networking;

public interface INetSynced
{
    public void SerializeCreate(BitPackedData data);
    public void Serialize(BitPackedData data);
    public void Deserialize(BitPackedData data);

}