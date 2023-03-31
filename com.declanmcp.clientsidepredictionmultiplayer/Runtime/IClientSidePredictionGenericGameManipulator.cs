
namespace ClientSidePredictionMultiplayer
{
    public interface IClientSidePredictionGenericGameManipulator<T> where T : INetworkGamePlayState
    {
        void SetPlayerInputsForTick(int tick);

        void AddForcesToGame();

        void SendGamePlayState(T gamePlayState);

        void SetGamePlayStateToState(T gamePlayState);

        T GetGamePlayStateWithTick(int stateTick);
    }
}
