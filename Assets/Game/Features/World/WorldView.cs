using Automathon.Engine;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
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