
namespace AM2E.Networking;

internal class ObjectDeletionEvent : NetReliableData
{
    public string ID;
    internal override void Deserialize(BitPackedData data)
    {
        ID = data.ReadID();
    }

    internal override void Serialize(BitPackedData data)
    {
        // Reliable message Type 3.
        data.WriteBits(3, 8);
        data.WriteID(ID);
    }
}