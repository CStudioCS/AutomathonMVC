using Automathon.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Input
{
    public class PlayerInputProvider : IInputProvider
    {
        public enum PlayerControlsType { LeftKeyboard, RightKeyboard, Gamepad }
        public PlayerControlsType PlayerControls { get; private set; }
        private InputAction dashAction;
        private InputAction grenadeAction;
        private InputAction shieldAction;
        private InputAction shootAction;
        private InputAction moveAction;
        private InputAction aimAction;

        public Dictionary<string, PlayerControlsType> schemeToControlsType = new Dictionary<string, PlayerControlsType>
        {
            { "Gamepad", PlayerControlsType.Gamepad },
            { "Keyboard_left", PlayerControlsType.LeftKeyboard },
            { "Keyboard_right", PlayerControlsType.RightKeyboard }
        };

        public PlayerInputProvider(PlayerInput playerInput)
        {
            PlayerControls = schemeToControlsType[playerInput.currentControlScheme];
            dashAction = playerInput.actions["Dash"];
            grenadeAction = playerInput.actions["Grenade"];
            shieldAction = playerInput.actions["Shield"];
            shootAction = playerInput.actions["Shoot"];
            moveAction = playerInput.actions["Move"];
            aimAction = playerInput.actions["Aim"];
        }

        public bool ShouldDash() => dashAction.IsPressed();

        public bool ShouldGrenade() => grenadeAction.IsPressed();

        public Vector2Int GetMilliMovementDir()
        {
            Vector2 movementDir = moveAction.ReadValue<Vector2>();
            return movementDir.ToVector2IntScaled();
        }

        public bool ShouldShield() => shieldAction.IsPressed();

        public bool ShouldShoot() => shootAction.IsPressed();

        public Vector2Int GetMilliAimingDir()
        {
            Vector2 aimingDir = aimAction.ReadValue<Vector2>();
            if (PlayerControls == PlayerControlsType.RightKeyboard)
            {
                Vector3 worldPos = aimingDir.ScreenToWorldSpace();
                return ((Vector2)worldPos).ToVector2IntScaled();
            }
            return new Vector2Int((int)(aimingDir.x * 1000), (int)(aimingDir.y * 1000));
        }
    }
}