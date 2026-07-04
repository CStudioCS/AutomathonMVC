using Automathon.Game.Input;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Automathon.Game
{
    public class InputManager : MonoBehaviour
    {
        public bool TryFindNewInput(out PlayerInputProvider inputProvider)
        {
            if (HandleGamepadJoin(out PlayerInputProvider p1))
            {
                inputProvider = p1;
                return true;
            }

            if (HandleKeyboardJoin(out PlayerInputProvider p2))
            {
                inputProvider = p2;
                return true;
            }

            inputProvider = null;
            return false;
        }

        private bool HandleKeyboardJoin(out PlayerInputProvider playerInputProvider)
        {
            PlayerInputProvider JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType controlsType)
            {
                foreach (InputProvider inputProvider in WorldView.Instance.InputProviders)
                {
                    if (inputProvider is PlayerInputProvider pInput && pInput.ControlsType == controlsType)
                        return null;
                }

                return new PlayerInputProvider(new InputDevice[] { Keyboard.current, Mouse.current }, controlsType);
            }


            playerInputProvider = null;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
            {
                playerInputProvider = JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.LeftKeyboard);
            }
            else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame
                || keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame
                || keyboard.rightShiftKey.wasPressedThisFrame || keyboard.slashKey.wasPressedThisFrame)
            {
                playerInputProvider = JoinKeyboardPlayer(PlayerInputProvider.PlayerControlsType.RightKeyboard);
            }

            return playerInputProvider != null;
        }

        private bool HandleGamepadJoin(out PlayerInputProvider playerInputProvider)
        {
            playerInputProvider = null;
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
                        if (inputProvider is PlayerInputProvider pInput && pInput.InputDevices.Contains(gamepad))
                            return false;
                    }

                    playerInputProvider = new PlayerInputProvider(new InputDevice[] { gamepad }, GetControllerType(gamepad));
                    return true;
                }
            }

            return false;
        }

        private static PlayerInputProvider.PlayerControlsType GetControllerType(Gamepad gamepad)
        {
            if (gamepad == null) return PlayerInputProvider.PlayerControlsType.Xbox;

            // 1. Check via exact Class Type Matching
            if (gamepad is XInputController)
                return PlayerInputProvider.PlayerControlsType.Xbox;

            if (gamepad is DualShockGamepad)
                return PlayerInputProvider.PlayerControlsType.PlayStation;

            // 2. Check via Layout Name (Best for Cross-Platform & Third-Party)
            string layoutName = gamepad.layout;

            if (layoutName.Contains("Xbox") || layoutName.Contains("XInput"))
                return PlayerInputProvider.PlayerControlsType.Xbox;

            if (layoutName.Contains("DualShock") || layoutName.Contains("DualSense") || layoutName.Contains("PlayStation"))
                return PlayerInputProvider.PlayerControlsType.PlayStation;

            if (layoutName.Contains("Switch") || layoutName.Contains("ProController"))
                return PlayerInputProvider.PlayerControlsType.Switch;

            // 3. Last Resort Fallback: Product Description Name
            string productName = gamepad.description.product?.ToLower() ?? "";
            if (productName.Contains("xbox")) return PlayerInputProvider.PlayerControlsType.Xbox;
            if (productName.Contains("sony") || productName.Contains("playstation")) return PlayerInputProvider.PlayerControlsType.PlayStation;
            if (productName.Contains("nintendo") || productName.Contains("switch")) return PlayerInputProvider.PlayerControlsType.Switch;

            return PlayerInputProvider.PlayerControlsType.Xbox;
        }
    }
}
