using Automathon.Engine;

namespace Automathon.Game.Input
{
    public abstract class InputProvider : Component
    {
        /// <summary>Desired movement direction. Its length (at most 1) is the fraction of the max speed to use.</summary>
        public abstract FixedVector2 GetMovementDir();
        /// <summary>Aiming direction (a unit vector, or zero when there's no aiming input).</summary>
        public abstract FixedVector2 GetAimingDir();
        public abstract bool ShouldShoot();
        public abstract bool ShouldShield();
        public abstract bool ShouldMissile();
        public abstract bool ShouldDash();
    }
}
