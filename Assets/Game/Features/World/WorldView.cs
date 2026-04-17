using Automathon.Engine;
using Automathon.Game.TankSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;

        private GameplayManager gameplayManager = new GameplayManager();

        private void Awake()
        {
            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            gameplayManager.Awake();
        }

        // Update is called once per frame
        void Update()
        {
            gameplayManager.Update();
        }
    }

}