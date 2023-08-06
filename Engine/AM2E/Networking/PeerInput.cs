
namespace AM2E.Networking;

internal class PeerInput
{
    internal int AcknowledgedTick = 0;
    internal int lastInput = -1;
    internal int Delay = -1;
    internal Controller controller = new Controller();
    internal readonly NetCommand[] InputBuffer = new NetCommand[NetworkGeneral.MaxGameSequence];
}