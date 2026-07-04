using Automathon.Game.View;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Automathon.Game.Input
{
    public class PlayerInputProvider : InputProvider
    {
        public enum PlayerControlsType { LeftKeyboard, RightKeyboard, Xbox, PlayStation, Switch }

        public InputDevice[] InputDevices;
        public PlayerControlsType ControlsType { get; private set; }
        private InputAction dashAction;
        private InputAction missileAction;
        private InputAction shieldAction;
        private InputAction shootAction;
        private InputAction moveAction;
        private InputAction aimAction;

        public static readonly Dictionary<PlayerControlsType, string> ControlsTypeToScheme = new Dictionary<PlayerControlsType, string>
        {
            { PlayerControlsType.Xbox, "Gamepad" },
            { PlayerControlsType.PlayStation, "Gamepad" },
            { PlayerControlsType.Switch, "Gamepad" },
            { PlayerControlsType.LeftKeyboard, "Keyboard_left" },
            { PlayerControlsType.RightKeyboard, "Keyboard_right" }
        };

        public PlayerInputProvider(InputDevice[] inputDevices, PlayerControlsType controlType)
        {
            this.InputDevices = inputDevices;
            ControlsType = controlType;
        }

        public void Setup(TankView tankView)
        {
            PlayerInput playerInput = tankView.GetComponent<PlayerInput>();

            playerInput.defaultControlScheme = ControlsTypeToScheme[ControlsType];

            foreach (var device in InputDevices)
                InputUser.PerformPairingWithDevice(device, playerInput.user);

            playerInput.enabled = true;

            playerInput.SwitchCurrentControlScheme(playerInput.defaultControlScheme, InputDevices);

            dashAction = playerInput.actions["Dash"];
            missileAction = playerInput.actions["Missile"];
            shieldAction = playerInput.actions["Shield"];
            shootAction = playerInput.actions["Shoot"];
            moveAction = playerInput.actions["Move"];
            aimAction = playerInput.actions["Aim"];
        }

        public override bool ShouldDash()
            => dashAction.WasPressedThisFrame();

        public override bool ShouldMissile()
            => missileAction.WasPressedThisFrame();

        public override Vector2Int GetMilliMovementDir()
        {
            Vector2 movementDir = moveAction.ReadValue<Vector2>();
            return movementDir.ToVector2IntScaled();
        }

        public override bool ShouldShield()
            => shieldAction.WasPressedThisFrame();

        public override bool ShouldShoot()
            => shootAction.IsPressed();

        public override Vector2Int GetMilliAimingDir()
        {
            Vector2 readInput = aimAction.ReadValue<Vector2>();

            if (ControlsType == PlayerControlsType.LeftKeyboard)
            {
                Vector2 mouseWorldPos = readInput.ScreenToWorldSpace();
                Vector2Int aimingVector = mouseWorldPos.ToVector2IntScaled() - ParentEntity.Position;

                if (aimingVector != Vector2Int.Zero)
                    aimingVector.NormalizeAtScale(1000);

                return aimingVector;
            }

            return readInput.ToVector2IntScaled();
        }
    }
}