using System;

namespace ClientSidePredictionMultiplayer
{
    public interface INetworkGamePlayStateDelegate<T> where T : INetworkGamePlayState
    {
        bool IsHost();

        T GetGamePlayStateWithTick(int stateTick);

        void SetGamePlayStateToState(T gamePlayState);

        void AddForcesToGame();

        void SendGamePlayState(T gamePlayState);
    }
}