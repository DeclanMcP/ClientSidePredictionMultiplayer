using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ClientSidePredictionMultiplayer
{
    public interface IClientSidePredictionGenericExecutor<T> where T : INetworkGamePlayState
    {
        void SetGameManipulator(IClientSidePredictionGenericGameManipulator<T> manipulator);

        void ResetPlayerInputsForTick(int tick);

        void SaveCurrentGamePlayStateWithTick(int tick);

        void ReplayPhysics(int currentTick);

        void ResetGameBuffers();

        void ReceivedGamePlayState(T gamePlayState);
    }
}