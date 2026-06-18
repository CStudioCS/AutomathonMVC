using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class MachineGunAbility : Ability
    {
        private const int COOLDOWN_MILLI = 2000;

        public int NumFiredBullets;
        public int TimeToFireAllMilli;

        public event Action BulletShot;

        public MachineGunAbility(int numFiredBullets, int timeToFireAllMilli, Func<bool> shouldActivate) : base(COOLDOWN_MILLI, shouldActivate)
        {
            NumFiredBullets = numFiredBullets;
            TimeToFireAllMilli = timeToFireAllMilli;
        }

        protected override void Activate()
        {
            int amountFired = 0;
            Tank.AddBehavior(new Timer(TimeToFireAllMilli, (t) =>
            {
                for (int i = amountFired; i < NumFiredBullets; i++)
                {
                    if ((t.MaxValueFrames - t.ValueFrames) * 1000 / GameplayConstants.FRAMERATE < i * TimeToFireAllMilli / NumFiredBullets)
                        return;

                    GameplayManager.Instantiate(new Bullet(Tank.Position + Tank.LastMilliDirection * Tank.SPAWN_DISTANCE_FROM_TANK / 1000, Tank.LastMilliDirection, Tank));
                    BulletShot?.Invoke();
                    amountFired++;
                }
            }));
        }
    }
}
