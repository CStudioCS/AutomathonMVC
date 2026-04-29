using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        private GameplayManager GameplayManager;

        private void Awake()
        {
            GameplayManager = new GameplayManager();
            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            TankView tankView = Instantiate(tankViewPrefab);
            Tank tank = new Tank(new Vector2Int(0, 0), new PlayerInputProvider(tankView.PlayerInput), GameplayManager);
            GameplayManager.Instantiate(tank);
            tankView.Initialize(tank);

            TankView tankView2 = Instantiate(tankViewPrefab);
            Tank tank2 = new Tank(new Vector2Int(5000, 0), new EmptyInputProvider(), GameplayManager);
            GameplayManager.Instantiate(tank2);
            tankView2.Initialize(tank2);
        }

        // Update is called once per frame
        void Update()
        {
            GameplayManager.Update();
        }
    }

}