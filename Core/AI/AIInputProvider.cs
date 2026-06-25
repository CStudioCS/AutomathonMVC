using Automathon.Game.Input;

namespace Automathon.AI
{
    public class AIInputProvider : IInputProvider
    {
        private AIAction lastAction;

        public AIInputProvider()
        {
            lastAction.MovingDirection = new Vector2Int(1000, 0);
            lastAction.AimingDirection = new Vector2Int(1000, 0);
        }

        public void UpdateFromAction(AIAction action)
        {
            void NormalizeAndCheckDir(ref Vector2Int dir, bool allowLower)
            {
                //prevent int overflow
                if (dir.X > 1000 * 1000)
                    dir.X = 1000;
                if (dir.Y > 1000 * 1000)
                    dir.Y = 1000;

                if (dir.LengthSquared() > 1000000 || !allowLower)
                    dir.NormalizeAtScale(1000);
            }

            NormalizeAndCheckDir(ref action.MovingDirection, true);
            NormalizeAndCheckDir(ref action.AimingDirection, false);

            //Attention : Data du user donc programmer defensivement pour eviter les erreurs
            if (action.AimingDirection == Vector2Int.Zero)
                action.AimingDirection = lastAction.AimingDirection;

            lastAction = action;
        }

        public Vector2Int GetMilliAimingDir()
            => lastAction.AimingDirection;

        public Vector2Int GetMilliMovementDir()
            => lastAction.MovingDirection;

        public bool ShouldDash()
            => lastAction.Dash;

        public bool ShouldMissile()
            => lastAction.Missile;

        public bool ShouldShield()
            => lastAction.Shield;

        public bool ShouldShoot()
            => lastAction.MachineGun;
    }

}
