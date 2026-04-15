using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Input
{
    public class PlayerInputProvider : IInputProvider
    {
        private PlayerInput playerInput;

        private InputAction dashAction;
        private InputAction grenadeAction;
        private InputAction shieldAction;
        private InputAction shootAction;
        private InputAction moveAction;

        public PlayerInputProvider(PlayerInput playerInput)
        {
            this.playerInput = playerInput;

            dashAction = playerInput.actions["Dash"];
            grenadeAction = playerInput.actions["Grenade"];
            shieldAction = playerInput.actions["Shield"];
            shootAction = playerInput.actions["Shoot"];
            moveAction = playerInput.actions["Move"];
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
    }
}