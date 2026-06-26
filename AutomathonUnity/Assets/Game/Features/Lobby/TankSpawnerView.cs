using Automathon.Engine;
using Automathon.Game.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game
{
    public class TankSpawnerView : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputManager;
        private List<Gamepad> usedControllers = new();

        private void Awake()
        {
            //playerInputManager.onPlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            //playerInputManager.onPlayerLeft -= OnPlayerLeft;
        }

        private void Update()
        {
            if (GameplayManager.State != GameplayManager.GameplayState.Lobby)
                return;

            if (PlayerInput.all.Count < GameplayConstants.MAX_PLAYERS)
            {
                HandleGamepadJoin();
                HandleKeyboardJoin();
            }


            /*if (Keyboard.current != null)
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
            }*/
        }

        private void HandleKeyboardJoin()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
            {
                JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.LeftKeyboard);
            }
            else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame
                || keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame
                || keyboard.rightShiftKey.wasPressedThisFrame || keyboard.slashKey.wasPressedThisFrame)
            {
                JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.RightKeyboard);
            }

            void JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType controlsType)
            {
                foreach (InputProvider inputProvider in WorldView.Instance.InputProviders)
                {
                    if (inputProvider is PlayerInputProvider pInput && pInput.ControlsType == controlsType)
                        return;
                }

                PlayerInputProvider playerInputProvider = new PlayerInputProvider(new InputDevice[] { Keyboard.current, Mouse.current }, controlsType);
                WorldView.Instance.InputProviders.Add(playerInputProvider);
                WorldView.Instance.OnPlayerJoined();
            }
        }

        private void HandleGamepadJoin()
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
                    foreach (InputProvider inputProvider in WorldView.Instance.InputProviders)
                    {
                        if (inputProvider is PlayerInputProvider pInput && pInput.ControlsType == PlayerInputProvider.PlayerControlsType.Gamepad && pInput.InputDevices.Contains(gamepad))
                            return;
                    }

                    PlayerInputProvider playerInputProvider = new PlayerInputProvider(new InputDevice[] { gamepad }, PlayerInputProvider.PlayerControlsType.Gamepad);
                    WorldView.Instance.InputProviders.Add(playerInputProvider);
                    WorldView.Instance.OnPlayerJoined();
                }
            }
        }

        public void OnPlayerLeft(PlayerInput playerInput)
        {

        }
    }
}
