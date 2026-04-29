using Automathon.Engine;
using Automathon.Game.BulletSystem;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private BulletView bulletViewPrefab;

        private GameplayManager gameplayManager = new();

        private void Awake()
        {
            Bullet.OnSpawn += SpawnBulletView;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            TankView tankView = Instantiate(tankViewPrefab);
            Tank tank = new Tank(new Vector2Int(0, 0), new PlayerInputProvider(tankView.PlayerInput));
            gameplayManager.Instantiate(tank);
            tankView.Initialize(tank);

            TankView tankView2 = Instantiate(tankViewPrefab);
            Tank tank2 = new Tank(new Vector2Int(5000, 0), new EmptyInputProvider());
            gameplayManager.Instantiate(tank2);
            tankView2.Initialize(tank2);

        }

        private void SpawnBulletView(Bullet bullet)
        {
            BulletView bulletView = Instantiate(bulletViewPrefab);
            bulletView.Initialize(bullet);
        }

        // Update is called once per frame
        void Update()
        {
            gameplayManager.Update();
        }

        private void OnDisable()
        {
            Bullet.OnSpawn -= SpawnBulletView;
        }
    }

}