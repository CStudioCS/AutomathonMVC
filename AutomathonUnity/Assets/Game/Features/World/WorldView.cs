using Automathon.Engine;
using Automathon.Game.View;
using UnityEngine;

namespace Automathon.Game
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private EntityViewRegistry entityViewRegistry;
        [SerializeField] private TankView tankView;

        private bool subbedToSpawnEntityView;

        private void Awake()
        {
            Debug.LogEvent += DebugForward;
            Debug.LogErrorEvent += DebugErrorForward;

            GameplayManager.Initialize();
            GameplayManager.EntitySpawned += SpawnEntityViewFromDict;

            GameplayManager.AIInputProvider = new AI.AIInputProvider();
            Tank tank = new Tank(Vector2Int.Right * 5000, GameplayManager.AIInputProvider);

            GameplayManager.Instantiate(tank);
            TankView t = Instantiate(tankView);
            t.Initialize(tank);

            subbedToSpawnEntityView = true;

            //ServerHandler.StartServer();

            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            QualitySettings.vSyncCount = 0;
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

        private void DebugForward(string message)
            => UnityEngine.Debug.Log(message);

        private void DebugErrorForward(string message)
            => UnityEngine.Debug.LogError(message);

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
            //ServerHandler.StopServer();
            GameplayManager.Dispose();

            Debug.LogEvent -= DebugForward;
            Debug.LogErrorEvent -= DebugErrorForward;
        }
    }
}