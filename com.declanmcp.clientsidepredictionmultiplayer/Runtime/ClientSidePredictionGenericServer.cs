using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ClientSidePredictionMultiplayer
{
    public class ClientSidePredictionGenericServer<T> : IClientSidePredictionGenericExecutor<T> where T : INetworkGamePlayState
    {
        private static int gameplayStateBufferSize = 1024;
        private T[] gameplayStateBuffer = new T[gameplayStateBufferSize];//For the host
        int latestSentGamePlayStateTick = 0;
        IClientSidePredictionGenericGameManipulator<T> manipulator;

        public void SetGameManipulator(IClientSidePredictionGenericGameManipulator<T> manipulator)
        {
            this.manipulator = manipulator;
        }

        public void ResetPlayerInputsForTick(int tick)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var clientPlayerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                var clientNeworkPlayerObject = clientPlayerObject.GetComponent<ClientSidePredictionGenericPlayer>();
                clientNeworkPlayerObject.ResetInputsForTick(tick);
            }
        }

        public void SaveCurrentGamePlayStateWithTick(int tick)
        {
            var gameplayState = GetGamePlayStateWithTick(tick);
            gameplayStateBuffer[tick % gameplayStateBufferSize] = gameplayState;
        }


        private T GetGamePlayStateWithTick(int stateTick)
        {
            return manipulator.GetGamePlayStateWithTick(stateTick);
        }

        public void ReplayPhysics(int currentTick)
        {
            //Keep track of the earliest tick received that contained input that hadnt been simulated
            //Resimulate from there.
            int latestSyncedInputTick = LatestSyncedInputStateTick();
            if (latestSyncedInputTick > latestSentGamePlayStateTick)
            {
                var replayTick = latestSentGamePlayStateTick;
                manipulator.SetGamePlayStateToState(gameplayStateBuffer[replayTick % gameplayStateBufferSize]);

                while (replayTick < currentTick)
                {
                    manipulator.SetPlayerInputsForTick(replayTick);
                    manipulator.AddForcesToGame();
                    Physics.Simulate(Time.fixedDeltaTime);
                    //Now we have resimulated this state, we need to save it again.
                    gameplayStateBuffer[replayTick % gameplayStateBufferSize] = GetGamePlayStateWithTick(replayTick);

                    if (latestSyncedInputTick == replayTick)
                    {
                        //Debug.Log("[SendState] tick: " + currentTick + ", replayTick: " + replayTick);
                        manipulator.SendGamePlayState(gameplayStateBuffer[replayTick % gameplayStateBufferSize]);
                        latestSentGamePlayStateTick = latestSyncedInputTick;
                    }

                    replayTick++;
                }
            }
        }


        private int LatestSyncedInputStateTick()
        {
            int currentLowestTick = int.MaxValue;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var clientPlayerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                var clientNeworkPlayerObject = clientPlayerObject.GetComponent<ClientSidePredictionGenericPlayer>();
                currentLowestTick = Math.Min(clientNeworkPlayerObject.latestInputTick, currentLowestTick);
            }
            if (currentLowestTick == int.MaxValue)
            {
                currentLowestTick = 0;
            }
            return currentLowestTick;
        }


        public void ResetGameBuffers()
        {
            gameplayStateBuffer = new T[gameplayStateBufferSize];
            latestSentGamePlayStateTick = 0;
            ResetPlayerInputBuffers();
        }

        void ResetPlayerInputBuffers()
        {
            foreach (KeyValuePair<ulong, NetworkObject> keyValuePair in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            {
                var clientPlayerObject = keyValuePair.Value;
                var clientNeworkPlayerObject = clientPlayerObject.GetComponent<ClientSidePredictionGenericPlayer>();
                if (clientNeworkPlayerObject != null)
                {
                    clientNeworkPlayerObject.ResetInputBuffer();
                }
            }
        }

        public void ReceivedGamePlayState(T gamePlayState)
        {
        }
    }
}