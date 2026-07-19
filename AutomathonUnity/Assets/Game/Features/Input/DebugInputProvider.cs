#if AUTOMATHON_DEBUG
using Automathon.Game.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.Input
{
    /// <summary>
    /// Editor / dev-only input provider that lets a single developer drive the whole
    /// match from one keyboard + mouse:
    ///   Move: WASD (or ZQSD) — Aim: mouse — Machine gun: LMB — Missile: RMB — Shield: E — Dash: Left-Shift
    ///
    /// Both tanks receive their own instance, but only the one whose <see cref="index"/>
    /// matches <see cref="ControlledIndex"/> reports live input; the other stays idle.
    /// <see cref="WorldView"/> flips <see cref="ControlledIndex"/> on Tab so you can hop
    /// between tanks without a second device.
    ///
    /// The whole file is gated behind the AUTOMATHON_DEBUG scripting define symbol, so it
    /// is compiled out of any build that does not define it.
    /// </summary>
    public class DebugInputProvider : InputProvider
    {
        // The player slot (0 = Green/Tank1, 1 = Red/Tank2) currently driven by the keyboard.
        // Shared by all debug providers so exactly one tank is live at a time.
        public static int ControlledIndex;

        private readonly int index;

        public DebugInputProvider(int index) => this.index = index;

        private bool IsControlled => ControlledIndex == index;

        public override Vector2Int GetMilliMovementDir()
        {
            Keyboard kb = Keyboard.current;
            if (!IsControlled || kb == null) return Vector2Int.Zero;

            Vector2 dir = Vector2.zero;
            if (kb.wKey.isPressed || kb.zKey.isPressed) dir.y += 1f; // W (QWERTY) / Z (AZERTY)
            if (kb.sKey.isPressed) dir.y -= 1f;
            if (kb.dKey.isPressed) dir.x += 1f;
            if (kb.aKey.isPressed || kb.qKey.isPressed) dir.x -= 1f; // A (QWERTY) / Q (AZERTY)

            if (dir != Vector2.zero)
                dir = dir.normalized;

            return dir.ToVector2IntScaled();
        }

        public override Vector2Int GetMilliAimingDir()
        {
            Mouse mouse = Mouse.current;
            if (!IsControlled || mouse == null) return Vector2Int.Zero;

            // Mirrors PlayerInputProvider's LeftKeyboard aim: mouse screen pos -> world -> direction from tank.
            Vector2 mouseWorldPos = mouse.position.ReadValue().ScreenToWorldSpace();
            Vector2Int aimingVector = mouseWorldPos.ToVector2IntScaled() - ParentEntity.Position;

            if (aimingVector != Vector2Int.Zero)
                aimingVector.NormalizeAtScale(1000);

            return aimingVector;
        }

        public override bool ShouldShoot()
            => IsControlled && Mouse.current != null && Mouse.current.leftButton.isPressed;

        public override bool ShouldMissile()
            => IsControlled && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        public override bool ShouldShield()
            => IsControlled && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        public override bool ShouldDash()
            => IsControlled && Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
    }
}
#endif
