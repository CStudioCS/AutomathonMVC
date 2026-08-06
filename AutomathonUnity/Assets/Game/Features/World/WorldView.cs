using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.View;
using UnityEngine;

namespace Automathon.Game
{
    public class WorldView : MonoBehaviour
    {
        private enum LobbyStates { Logo, Input, End }


        public static WorldView Instance;
        private LobbyStates LobbyState;

        [SerializeField] private EntityViewRegistry entityViewRegistry;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private GameObject inputTakingMenu;
        [SerializeField] private EndScreen endCard;

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
            GameplayManager.GameEnded += OnEndGame;

            InputProviders = new InputProvider[] { null, null };

            subbedToSpawnEntityView = true;

            Application.targetFrameRate = GameplayConstants.FRAMERATE;
            QualitySettings.vSyncCount = 0;

            LobbyState = LobbyStates.Logo;

            playButton.gameObject.SetActive(true);
            playButton.onClick.AddListener(OnStartButtonClicked);
        }

        private void OnStartButtonClicked()
        {
            SoundManager.instance.PlaySound("MenuButton");
            playButton.gameObject.SetActive(false);

            LobbyState = LobbyStates.Input;
            inputTakingMenu.SetActive(true);
        }

        private void SpawnEntityViewFromDict(Entity entity)
        {
            EntityView entityViewPrefab = entityViewRegistry.GetPrefabFor(entity.GetType());

            if (entityViewPrefab == null)
            {
                //UnityEngine.Debug.LogError($"No view registered for {entity.GetType().Name}");
                //pk une error hein on est pas obligé de spawn un view pour chaque entity, on peut juste pas en avoir besoin
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

            inputTakingMenu.SetActive(false);
            GameplayManager.Reset(InputProviders[0], InputProviders[1]);
        }

        private void Update()
        {
#if AUTOMATHON_DEBUG
            HandleDebugInput();
#endif

            GameplayManager.Update();
        }


#if AUTOMATHON_DEBUG
        // Dev-only solo play (gated by the AUTOMATHON_DEBUG scripting define symbol):
        //   F1  from the lobby  -> start a match driven entirely by keyboard + mouse
        //   Tab during a match  -> switch which tank the keyboard + mouse controls
        private void HandleDebugInput()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            if (GameplayManager.State == GameplayManager.GameplayState.Lobby)
            {
                if (keyboard.f1Key.wasPressedThisFrame)
                    StartDebugGame();
            }
            else if (keyboard.tabKey.wasPressedThisFrame) // GameplayState.Game
            {
                int next = 1 - Automathon.Game.Input.DebugInputProvider.ControlledIndex;
                Automathon.Game.Input.DebugInputProvider.ControlledIndex = next;
                UnityEngine.Debug.Log($"[DEBUG] Now controlling tank #{next} ({(next == 0 ? "Green" : "Red")})");
            }
        }

        private void StartDebugGame()
        {
            playButton.gameObject.SetActive(false);

            Automathon.Game.Input.DebugInputProvider.ControlledIndex = 0;
            InputProviders[0] = new Automathon.Game.Input.DebugInputProvider(0);
            InputProviders[1] = new Automathon.Game.Input.DebugInputProvider(1);

            StartGame(); // hides the input menu and calls GameplayManager.Reset with both providers

            UnityEngine.Debug.Log("[DEBUG] Solo match started. WASD/ZQSD move, mouse aim, LMB gun, " +
                "RMB missile, E shield, Left-Shift dash. Press Tab to switch tanks.");
        }
#endif

        private void OnEndGame(Tank.TeamType winner)
        {
            endCard.Scroll(winner);
            LobbyState = LobbyStates.End;
        }

        public void OnEndScreenDone()
        {
            LobbyState = LobbyStates.Input;
            inputTakingMenu.SetActive(true);
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
                GameplayManager.GameEnded += OnEndGame;
                subbedToSpawnEntityView = true;
            }
        }

        private void OnDisable()
        {
            if (subbedToSpawnEntityView)
            {
                GameplayManager.EntitySpawned -= SpawnEntityViewFromDict;
                GameplayManager.GameEnded -= OnEndGame;
                subbedToSpawnEntityView = false;
            }
        }

        private void OnDestroy()
        {
            //ServerHandler.StopServer();
            GameplayManager.Dispose();

            Debug.LogEvent -= DebugForward;
            Debug.LogErrorEvent -= DebugErrorForward;
            playButton.onClick.RemoveAllListeners();
        }
    }
}