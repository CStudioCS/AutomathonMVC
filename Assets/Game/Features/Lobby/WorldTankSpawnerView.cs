using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Lobby
{
    public class WorldTankSpawnerView : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private TankView tankViewPrefab;
        [SerializeField] private GameObject tankPrefab;
        public event Action<PlayerInput> PlayerJoined;

        List<PlayerInput> players = new();
        public static WorldTankSpawnerView Instance { get; private set; }
        private bool hasGamepadPlayerConnected = false;
        private bool hasKeyboardPlayerConnected = false;
        public void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }

        private void Update()
        {
            if (GameplayManager.Instance.State != GameplayManager.GameState.Lobby)
                return;

            HandleGamepadJoinInput();
            HandleKeyboardJoinInput();
        }
        private void HandleKeyboardJoinInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
            {
                JoinKeyboardPlayer();
            }
        }
        private void HandleGamepadJoinInput()
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.buttonWest.wasPressedThisFrame ||
                    gamepad.buttonNorth.wasPressedThisFrame ||
                    gamepad.leftShoulder.wasPressedThisFrame ||
                    gamepad.rightShoulder.wasPressedThisFrame ||
                    gamepad.startButton.wasPressedThisFrame ||
                    gamepad.selectButton.wasPressedThisFrame ||
                    gamepad.dpad.up.wasPressedThisFrame ||
                    gamepad.dpad.down.wasPressedThisFrame ||
                    gamepad.dpad.left.wasPressedThisFrame ||
                    gamepad.dpad.right.wasPressedThisFrame ||
                    gamepad.leftStick.ReadValue().magnitude > 0.1f ||
                    gamepad.rightStick.ReadValue().magnitude > 0.1f)
                {
                    JoinGamepadPlayer(gamepad);
                }
            }
        }
        private void JoinGamepadPlayer(Gamepad gamepad)
        {
            if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
            {
                //This kept triggering on accident everytime I maximized Unity so I commented it (why ?)
                //Debug.Log("An extra gamepad player tried to connect but player limit has been reached");
                return;
            }

            // Check if THIS specific controller is already assigned to a player
            foreach (var player in PlayerInput.all)
            {
                foreach (var device in player.devices)
                {
                    if (device == gamepad)
                        return;
                }
            }

            // Join using the Gamepad device. 
            // Leave controlScheme null or set to "Gamepad" if you have a specific scheme named that.

            PlayerInput playerInput = playerInputManager.JoinPlayer(
            playerIndex: -1,
            splitScreenIndex: -1,
            controlScheme: "Gamepad",
            pairWithDevice: gamepad);

            TankView tankView = playerInput.GetComponent<TankView>();
            tankView.PlayerInput = playerInput;
            Tank tank = new Tank(new Automathon.Vector2Int(0, 0), new PlayerInputProvider(tankView.PlayerInput));

            GameplayManager.Instance.Instantiate(tank);
            tankView.Initialize(tank);
        }
        private void JoinKeyboardPlayer()
        {
            // 1. Check if we are already at the player limit
            if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
            {
                //This kept triggering on accident everytime I maximized Unity so I commented it (why ?)
                //Debug.Log("An extra keyboard tried to connect but player limit has been reached");
                return;
            }

            foreach (PlayerInput playerInputTemp in PlayerInput.all)
            {
                if (playerInputTemp.currentControlScheme == "Keyboard&Mouse")
                    return;
            }
            // 2. Manually trigger the join
            // We pass -1 for playerIndex to let Unity assign the next available index (0, 1, 2, etc.)
            PlayerInput playerInput = playerInputManager.JoinPlayer(
                playerIndex: -1,
                splitScreenIndex: -1,
                controlScheme: "Keyboard&Mouse",
                pairWithDevice: Keyboard.current
            );

            TankView tankView = playerInput.GetComponent<TankView>();
            tankView.PlayerInput = playerInput;
            Tank tank = new Tank(new Automathon.Vector2Int(0, 0), new PlayerInputProvider(tankView.PlayerInput));

            GameplayManager.Instance.Instantiate(tank);
            tankView.Initialize(tank);
        }
        public void OnPlayerJoined(PlayerInput playerInput)
        {

            if (playerInput == null) return;

            playerInput.transform.position = new Vector3(2, 0, 0);

            players.Add(playerInput);
        }
        public void OnPlayerLeft(PlayerInput playerInput)
        {
            if (playerInput == null) return;

            TankView tankView = playerInput.GetComponent<TankView>();
            tankView.PlayerInput = null;
            Debug.Log($"Player {playerInput.playerIndex} left the lobby.");

        }
    }
}
