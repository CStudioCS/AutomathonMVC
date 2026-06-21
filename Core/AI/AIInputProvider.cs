using Automathon.Game.Input;

namespace Automathon.AI
{
    public class AIInputProvider : IInputProvider
    {
        private AIAction lastAction;
        public void UpdateFromAction(AIAction action)
        {
            //prevent int overflow
            void CheckDir(ref Vector2Int dir)
            {
                if (dir.X > 1000 * 1000)
                    dir.X = 1000;
                if (dir.Y > 1000 * 1000)
                    dir.Y = 1000;
            }

            CheckDir(ref action.MovingDir);
            CheckDir(ref action.AimingDir);
            CheckDir(ref action.DashDir);


            //Attention : Data du user donc programmer defensivement pour eviter les erreurs
            if (action.MovingDir == Vector2Int.Zero)
                action.MovingDir = lastAction.MovingDir;

            if (action.AimingDir == Vector2Int.Zero)
                action.AimingDir = lastAction.AimingDir;

            lastAction = action;
        }

        public Vector2Int GetMilliAimingDir()
            => lastAction.AimingDir;

        public Vector2Int GetMilliMovementDir()
            => lastAction.MovingDir;

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
