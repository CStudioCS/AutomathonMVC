using Automathon.Engine;
using Automathon.Game.TankSystem;
using Automathon.Game.View;
using Automathon.Game.View.Registry;
using UnityEngine;

namespace Automathon.Game.MapSystem
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private EntityViewRegistry entityViewRegistry;

        private bool subbedToSpawnEntityView;



        private void Awake()
        {
            GameplayManager.Initialize();
            GameplayManager.EntitySpawned += SpawnEntityViewFromDict;
            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            /*Map map1 = new Map("map1", new List<Entity> { new Wall(new Vector2Int(3000, 2000), new Vector2Int(6000, 2000), 1000), new Wall(new Vector2Int(-3000, -2000), new Vector2Int(6000, 2000), 1000) });

            MapSaver.RegisterMap(map1);
            Map map = MapSaver.LoadMap("map1");

            if (map != null)
            {
                MapGenerator.InstantiateMap(map);
            }
            else
            {
                Debug.Log("Failed to load map 'map1'. Skipping map instantiation.");
            }
            */
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