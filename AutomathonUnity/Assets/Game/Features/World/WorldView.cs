using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.View;
using UnityEngine;

namespace Automathon.Game
{
    public class WorldView : MonoBehaviour
    {
        public static WorldView Instance;
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private EntityViewRegistry entityViewRegistry;
        [SerializeField] private TankView tankView;

        public InputProvider[] InputProviders;

        private bool subbedToSpawnEntityView;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            Debug.LogEvent += DebugForward;
            Debug.LogErrorEvent += DebugErrorForward;

            GameplayManager.Initialize();
            GameplayManager.EntitySpawned += SpawnEntityViewFromDict;

            InputProviders = new InputProvider[] { null, null };

            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            QualitySettings.vSyncCount = 0;
        }

        private void SpawnEntityViewFromDict(Entity entity)
        {
            EntityView entityViewPrefab = entityViewRegistry.GetPrefabFor(entity.GetType());

            if (entityViewPrefab == null)
            {
                UnityEngine.Debug.LogError($"No view registered for {entity.GetType().Name}");
                return;
            }

            EntityView entityView = Instantiate(entityViewPrefab, entity.Position.ToVector2Scaled(), ViewMath.MilliRadRotationToQuaternion(entity.RotationMilli));
            entityView.Initialize(entity);
        }

        public void StartGame()
        {
            if (InputProviders[0] == null || InputProviders[1] == null)
            {
                Debug.LogError("Tried to start game without two input providers given");
                return;
            }

            GameplayManager.Reset(InputProviders[0], InputProviders[1]);
        }

        void Update()
        {
            if (GameplayManager.State == GameplayManager.GameplayState.Game)
            {
                GameplayManager.Update();

            }
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