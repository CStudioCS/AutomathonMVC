using Automathon.Engine;
using Automathon.Game.BulletSystem;
using Automathon.Game.GrenadeSystem;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private BulletView bulletViewPrefab;
        [SerializeField] private GrenadeView grenadeViewPrefab;

        private GameplayManager GameplayManager;

        private void Awake()
        {
            GameplayManager = new();
            Bullet.Spawned += SpawnBulletView;
            Grenade.OnSpawned += SpawnGrenadeView; ;

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

            /*Grenade grenade = new Grenade(new Vector2Int(1000, 1000), new Vector2Int(1000, 0), 1800, 3000, 12);
            gameplayManager.Instantiate(grenade);*/

        }

        private void SpawnBulletView(Bullet bullet)
        {
            BulletView bulletView = Instantiate(bulletViewPrefab);
            bulletView.Initialize(bullet);
        }

        private void SpawnGrenadeView(Grenade grenade)
        {
            GrenadeView grenadeView = Instantiate(grenadeViewPrefab);
            grenade.gameplayManager = gameplayManager;
            grenadeView.Initialize(grenade);
        }

        // Update is called once per frame
        void Update()
        {
            GameplayManager.Update();
        }

        private void OnDisable()
        {
            Bullet.Spawned -= SpawnBulletView;
            Grenade.OnSpawned -= SpawnGrenadeView;
        }
    }

}