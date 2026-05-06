using Automathon.Engine;
using Automathon.Game.MapSystem;
using Automathon.Game.TankSystem;
using Automathon.Game.View;
using Automathon.Game.View.Registry;
using Automathon.Game.WallSystem;
using System.Collections.Generic;
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
            GameplayManager.Initialize();
            GameplayManager.EntitySpawned += SpawnEntityViewFromDict;
            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;

            MapGenerator.InstantiateMap(MapSaver.LoadMap("map1"));
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