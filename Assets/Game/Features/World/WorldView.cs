using Automathon.Engine;
using Automathon.Game.GrenadeSystem;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using Automathon.Game.View;
using Automathon.Game.View.Registry;
using Automathon.Game.WallSystem;
using UnityEngine;

namespace Automathon.Game.World
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private EntityViewRegistry entityViewRegistry;

        private bool subbedToSpawnEntityView;

        private void Awake()
        {
            GameplayManager.EntitySpawned += SpawnEntityViewFromDict;
            GameplayManager.Initialize();
            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            //Tanks are spawned using Cedric's future system, not through auto instantiation (or maybe we should ?)
            //We can't spawn them automatically cuz we need the reference to player input
            TankView tankView = Instantiate(tankViewPrefab);
            Tank tank = new Tank(new Vector2Int(0, 0), new PlayerInputProvider(tankView.PlayerInput));
            GameplayManager.Instantiate(tank);
            tankView.Initialize(tank);

            TankView tankView2 = Instantiate(tankViewPrefab);
            Tank tank2 = new Tank(new Vector2Int(5000, 0), new EmptyInputProvider());
            GameplayManager.Instantiate(tank2);
            tankView2.Initialize(tank2);

            GameplayManager.Instantiate(new Grenade(new Vector2Int(1000, 1000), new Vector2Int(1000, 0)));
            GameplayManager.Instantiate(new Grenade(new Vector2Int(2000, 2000), new Vector2Int(1000, 0)));

            Wall wall = new Wall(new Vector2Int(-1000, 3000), new Vector2Int(3000, 500), 200);
            GameplayManager.Instantiate(wall);
        }

        private void SpawnEntityViewFromDict(Entity entity)
        {
            EntityView entityViewPrefab = entityViewRegistry.GetPrefabFor(entity.GetType());

            if (entityViewPrefab == null)
            {
                //Commented this since some entities are not auto spawned
                //UnityEngine.Debug.LogError($"No view registered for {entity.GetType().Name}");
                return;
            }

            EntityView entityView = Instantiate(entityViewPrefab);
            entityView.Initialize(entity);
        }

        // Update is called once per frame
        void Update()
        {
            GameplayManager.Update();
        }

        private void OnEnable()
        {
            if (!subbedToSpawnEntityView)
            {
                GameplayManager.EntitySpawned += SpawnEntityViewFromDict;
                subbedToSpawnEntityView = true;
            }
        }

        private void OnDisable()
        {
            if (subbedToSpawnEntityView)
            {
                GameplayManager.EntitySpawned -= SpawnEntityViewFromDict;
                subbedToSpawnEntityView = false;
            }
        }

        private void OnDestroy()
        {
            GameplayManager.Dispose();
        }
    }
}