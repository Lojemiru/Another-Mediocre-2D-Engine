
namespace AM2E.Networking;

public interface INetControllable
{
    public Controller controller { get; set; }
    public int master { get; set; }
}