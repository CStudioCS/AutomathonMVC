using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.Lobby.MultiTankManagement;
using Automathon.Game.TankSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Lobby
{
    public class TankSpawnerView : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        private Dictionary<PlayerInput, IInputProvider> inputProviders = new();

        private void Update()
        {
            if (GameplayManager.State != GameplayManager.GameState.Lobby)
                return;

            HandleGamepadJoinInput();
            HandleKeyboardJoinInput();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                foreach (PlayerInput playerInput in inputProviders.Keys)
                {
                    if (playerInput.currentControlScheme == "Keyboard&Mouse")
                        PlayerManager.PlayerLeft?.Invoke(inputProviders[playerInput]);
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
        private void JoinKeyboardPlayer()
        {
            if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
                return;

            foreach (PlayerInput playerInputTemp in inputProviders.Keys)
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

            SpawnTank(playerInput);
        }

        private void SpawnTank(PlayerInput playerInput)
        {
            inputProviders[playerInput] = new PlayerInputProvider(playerInput);

            Tank tank = PlayerManager.PlayerJoined?.Invoke(inputProviders[playerInput]);
            playerInput.GetComponent<TankView>().Initialize(tank);
        }
    }
}
