using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using Automathon.Game.WallSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private WallView wallViewPrefab;

        private GameplayManager gameplayManager = new();

        private void Awake()
        {
            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            TankView tankView = Instantiate(tankViewPrefab);
            Tank tank = new Tank(new Vector2Int(-5000, 0), new PlayerInputProvider(tankView.PlayerInput));
            gameplayManager.Instantiate(tank);
            tankView.Initialize(tank);

            TankView tankView2 = Instantiate(tankViewPrefab);
            Tank tank2 = new Tank(new Vector2Int(5000, 0), new EmptyInputProvider());
            gameplayManager.Instantiate(tank2);
            tankView2.Initialize(tank2);

            WallView wallView = Instantiate(wallViewPrefab);
            Wall wall = new Wall(new Vector2Int(0, 0), 5, 2);
            gameplayManager.Instantiate(wall);
            wallView.Initialize(wall);
        }

        // Update is called once per frame
        void Update()
        {
            gameplayManager.Update();
        }
    }

}