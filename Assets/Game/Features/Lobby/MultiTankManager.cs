using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Lobby.MultiTankManagement
{
    public class MultiTankManager : MonoBehaviour
    {
        public void Start()
        {
            WorldTankSpawnerView.Instance.PlayerJoined += OnPlayerJoined;
        }
        public void OnPlayerJoined(PlayerInput playerInput)
        {

        }
        private void OnDisable()
        {
            WorldTankSpawnerView.Instance.PlayerJoined -= OnPlayerJoined;
        }
    }
}
