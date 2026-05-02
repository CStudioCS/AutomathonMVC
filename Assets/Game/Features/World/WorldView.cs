using Automathon.Engine;
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

        private GameplayManager GameplayManager;

        private bool subbedToSpawnEntityView;

        private void Awake()
        {
            Entity.OnSpawned += SpawnEntityViewFromDict;
            GameplayManager = new();
            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            //this is ugly and temporary, let me be
            //Tanks are spawned using Cédrics future system, not through auto instantiation (or maybe we should ?)
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

            Wall wall = new Wall(new Vector2Int(3200, 2600), new Vector2Int(1500, 500), 200);
            GameplayManager.Instantiate(wall);
        }

        private void SpawnEntityViewFromDict(Entity entity)
        {
            EntityView entityViewPrefab = entityViewRegistry.GetPrefabFor(entity.GetType());

            if (entityViewPrefab == null)
            {
                UnityEngine.Debug.LogError($"No view registered for {entity.GetType().Name}");
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
                Entity.OnSpawned += SpawnEntityViewFromDict;
                subbedToSpawnEntityView = true;
            }
        }

        private void OnDisable()
        {
            if (subbedToSpawnEntityView)
            {
                Entity.OnSpawned -= SpawnEntityViewFromDict;
                subbedToSpawnEntityView = false;
            }
        }
    }
}