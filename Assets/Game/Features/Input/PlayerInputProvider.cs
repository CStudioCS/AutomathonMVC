using Automathon.Game.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Input
{
    public class PlayerInputProvider : IInputProvider
    {
        public enum PlayerControlsType { Left, Right, Gamepad }
        public PlayerControlsType PlayerControls { get; private set; }
        private InputAction dashAction;
        private InputAction grenadeAction;
        private InputAction shieldAction;
        private InputAction shootAction;
        private InputAction moveAction;
        private InputAction aimAction;
        public PlayerInputProvider(PlayerInput playerInput)
        {
            if (playerInput.currentControlScheme == "Gamepad")
            {
                PlayerControls = PlayerControlsType.Gamepad;
            }
            else if (playerInput.currentControlScheme == "Keyboard_left")
            {
                PlayerControls = PlayerControlsType.Left;
            }
            else if (playerInput.currentControlScheme == "Keyboard_right")
            {
                PlayerControls = PlayerControlsType.Right;
            }
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
            return new Vector2Int((int)(movementDir.x * 1000), (int)(movementDir.y * 1000));
        }

        public bool ShouldShield() => shieldAction.IsPressed();

        public bool ShouldShoot() => shootAction.IsPressed();

        public Vector2Int GetMilliAimingDir()
        {
            Vector2 aimingDir = aimAction.ReadValue<Vector2>();
            if (PlayerControls == playerControlsType.Right)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(aimingDir.x, aimingDir.y, WorldConstants.CAMERA_DISTANCE / 1000));
                return new Vector2Int((int)(worldPos.x * WorldConstants.SPACE_SCALE), (int)(worldPos.y * WorldConstants.SPACE_SCALE));
            }
            return new Vector2Int((int)(aimingDir.x * 1000), (int)(aimingDir.y * 1000));
        }
    }
}