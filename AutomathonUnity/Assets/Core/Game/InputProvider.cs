using Automathon.Engine;

namespace Automathon.Game.Input
{
    public abstract class InputProvider : Component
    {
        public abstract Vector2Int GetMilliMovementDir();
        public abstract Vector2Int GetMilliAimingDir();
        public abstract bool ShouldShoot();
        public abstract bool ShouldShield();
        public abstract bool ShouldMissile();
        public abstract bool ShouldDash();
    }
}
