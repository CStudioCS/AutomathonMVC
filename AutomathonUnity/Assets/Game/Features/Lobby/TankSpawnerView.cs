using Automathon.Engine;
using Automathon.Game.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game
{
    public class TankSpawnerView : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        private Dictionary<PlayerInput, IInputProvider> inputProviders = new();

        private void Awake()
        {
            playerInputManager.onPlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            playerInputManager.onPlayerLeft -= OnPlayerLeft;
        }

        private void Update()
        {
            if (GameplayManager.State != GameplayManager.GameplayState.Lobby)
                return;

            HandleGamepadJoinInput();
            HandleKeyboardJoinInput();

            if (Keyboard.current != null)
            {
                bool somebodyQuit = false;
                PlayerInputProvider.PlayerControlsType whichControlsQuit = default;
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    somebodyQuit = true;
                    whichControlsQuit = PlayerInputProvider.PlayerControlsType.LeftKeyboard;
                }
                else if (Keyboard.current.deleteKey.wasPressedThisFrame)
                {
                    somebodyQuit = true;
                    whichControlsQuit = PlayerInputProvider.PlayerControlsType.RightKeyboard;
                }
                if (somebodyQuit)
                {
                    PlayerInput playerInputToRemove = null;
                    foreach (PlayerInput playerInput in inputProviders.Keys)
                    {
                        if (PlayerInputProvider.SchemeToControlsType[playerInput.currentControlScheme] == whichControlsQuit)
                        {
                            playerInputToRemove = playerInput;
                            break;
                        }
                    }

                    if (playerInputToRemove != null)
                        RemovePlayerLeftInput(playerInputToRemove);
                }
            }
        }

        private void HandleKeyboardJoinInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
            {
                JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.LeftKeyboard);
            }
            else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame ||
                     keyboard.rightArrowKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame || keyboard.slashKey.wasPressedThisFrame)
            {
                JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.RightKeyboard);
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
                return;

            // Check if THIS specific controller is already assigned to a player
            foreach (PlayerInput player in inputProviders.Keys)
            {
                foreach (InputDevice device in player.devices)
                {
                    if (device == gamepad)
                        return;
                }
            }

            //Spawns a player
            PlayerInput playerInput = playerInputManager.JoinPlayer(
                playerIndex: -1,
                splitScreenIndex: -1,
                controlScheme: "Gamepad",
                pairWithDevice: gamepad
                );

            SpawnTank(playerInput);
        }

        private void JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType controlsType)
        {
            if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
                return;

            foreach (PlayerInput playerInputTemp in inputProviders.Keys)
            {
                if (PlayerInputProvider.SchemeToControlsType[playerInputTemp.currentControlScheme] == controlsType)
                    return;
            }

            // 2. Manually trigger the join
            // We pass -1 for playerIndex to let Unity assign the next available index (0, 1, 2, etc.)
            string controlScheme = PlayerInputProvider.ControlsTypeToScheme[controlsType];
            PlayerInput playerInput = playerInputManager.JoinPlayer(
                playerIndex: -1,
                splitScreenIndex: -1,
                controlScheme: controlScheme,
                pairWithDevice: Keyboard.current
            );

            SpawnTank(playerInput);
        }

        private void SpawnTank(PlayerInput playerInput)
        {
            inputProviders[playerInput] = new PlayerInputProvider(playerInput);

            Tank tank = PlayerManager.OnPlayerJoined(inputProviders[playerInput]);
            playerInput.GetComponent<TankView>().Initialize(tank);
        }

        public void OnPlayerLeft(PlayerInput playerInput)
        {
            RemovePlayerLeftInput(playerInput);
        }

        public void RemovePlayerLeftInput(PlayerInput playerInput)
        {
            if (inputProviders.ContainsKey(playerInput))
            {
                PlayerManager.OnPlayerLeft(inputProviders[playerInput]);
                inputProviders.Remove(playerInput);
            }
        }
    }
}
