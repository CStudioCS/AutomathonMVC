using UnityEngine;
using UnityEngine.InputSystem;

//Do I need to separate the namspaces for stuff that includes unity ? idk how that'll work out at compile times
namespace Automathon.Game.Input
{
    public class PlayerInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private PlayerInput playerInput;

        private InputAction dashAction;
        private InputAction grenadeAction;
        private InputAction shieldAction;
        private InputAction shootAction;
        private InputAction moveAction;

        private void Awake()
        {
            dashAction = playerInput.actions["Dash"];
            grenadeAction = playerInput.actions["Grenade"];
            shieldAction = playerInput.actions["Shield"];
            shootAction = playerInput.actions["Shoot"];
            moveAction = playerInput.actions["Move"];
        }

        public bool Dash() => dashAction.IsPressed();

        public bool Grenade() => grenadeAction.IsPressed();

        public Vector2Int MilliMovementDir()
        {
            var move = moveAction.ReadValue<Vector2>();
            return new Vector2Int((int)(move.x * 1000), (int)(move.y * 1000));
        }

        public bool Shield() => shieldAction.IsPressed();

        public bool Shoot() => shootAction.IsPressed();
    }
}