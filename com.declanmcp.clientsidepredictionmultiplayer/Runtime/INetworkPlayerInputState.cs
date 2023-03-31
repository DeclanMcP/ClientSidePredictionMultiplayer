
using Unity.Netcode;

namespace ClientSidePredictionMultiplayer
{
    public interface INetworkPlayerInputState: INetworkSerializable
    {
        int Tick { get; set; }
    }
}
