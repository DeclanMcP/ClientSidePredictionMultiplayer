using Unity.Netcode;
using UnityEngine;

namespace ClientSidePredictionMultiplayer
{
    abstract public class ClientSidePredictionGenericPlayer : NetworkBehaviour
    {
        private static int playerInputBufferSize = 1024;
        private INetworkPlayerInputState[] m_playerInputBuffer = new INetworkPlayerInputState[playerInputBufferSize];
        public int latestInputTick = 0;

        public ClientSidePredictionGenericPlayer()
        {
        }

        public void RecordPlayerInputForTick(int tick)
        {
            if (!IsOwner) { return; }
            var playerInputState = GetPlayerInputState();
            playerInputState.Tick = tick;
            if (playerInputState == null) { return; }

            StorePlayerInputState(playerInputState);

            if (IsHost)
            {
                return;
            }
            SendAndStoreToServer(playerInputState);
        }

        protected void StorePlayerInputState(INetworkPlayerInputState inputState)
        {
            m_playerInputBuffer[inputState.Tick % playerInputBufferSize] = inputState;
            latestInputTick = Mathf.Max(latestInputTick, inputState.Tick);
        }

        protected INetworkPlayerInputState GetPlayerInputStateForTick(int tick)
        {
            return m_playerInputBuffer[tick % playerInputBufferSize];
        }

        public void ResetInputsForTick(int tick)
        {
            m_playerInputBuffer[tick % playerInputBufferSize] = null;
        }

        public void ResetInputBuffer()
        {
            m_playerInputBuffer = new INetworkPlayerInputState[playerInputBufferSize];//TODO problematic
            latestInputTick = 0;
        }

        abstract public void UpdatePlayerInput();

        abstract public void SendAndStoreToServer(INetworkPlayerInputState inputState);

        abstract public INetworkPlayerInputState GetPlayerInputState();

        abstract public void SetInputsForTick(int tick);

        abstract public void AddForces();
    }
}
