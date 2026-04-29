using Automathon.Engine;
using Automathon.Game.Lobby;
using Automathon.Game.Lobby.MultiTankManagement;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        GameplayManager gameplayManager = new GameplayManager();
        MultiTankManager multiTankManager = new MultiTankManager();
        private void Awake()
        {
            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            gameplayManager.Awake();
        }
        //On fait un Start pas un Awake car on a besoin qhe WorldTanSpawnerView soit Awake
        private void Start()
        {
            multiTankManager.Start();
        }
        void Update()
        {
            gameplayManager.Update();
            multiTankManager.IsGameReady();
            TankSpawnerView.Instance.TrySetPlayersReady();
        }

    }

}