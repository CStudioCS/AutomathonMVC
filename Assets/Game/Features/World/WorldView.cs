using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.Tank;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;

        private GameplayManager gameplayManager = new();

        private void Awake()
        {
            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            TankView tankView = Instantiate(tankViewPrefab);
            PlayerInputProvider playerInputProvider = tankView.GetComponent<PlayerInputProvider>();
            Tank tank = new Tank(new Automathon.Vector2Int(0, 0), playerInputProvider);
            gameplayManager.Instantiate(tank);
            tankView.Initialize(tank);

            TankView tankView2 = Instantiate(tankViewPrefab);
            Tank tank2 = new Tank(new Automathon.Vector2Int(5000, 0), new EmptyInputProvider());
            gameplayManager.Instantiate(tank2);
            tankView2.Initialize(tank2);
        }

        // Update is called once per frame
        void Update()
        {
            gameplayManager.Update();
        }
    }

}