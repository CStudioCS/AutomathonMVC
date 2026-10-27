using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class MachineGunAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 2; // seconds

        public int NumFiredBullets;
        /// <summary>Time it takes to fire every bullet of a burst, in seconds.</summary>
        public Fixed TimeToFireAll;

        public event Action BulletShot;
        public event Action MachineGunFired;

        public MachineGunAbility(int numFiredBullets, Fixed timeToFireAll, Func<bool> shouldActivate) : base(COOLDOWN, shouldActivate)
        {
            NumFiredBullets = numFiredBullets;
            TimeToFireAll = timeToFireAll;
        }

        protected override void Activate()
        {
            MachineGunFired?.Invoke();
            int amountFired = 0;
            Tank.AddBehavior(new Timer(TimeToFireAll, (t) =>
            {
                for (int i = amountFired; i < NumFiredBullets; i++)
                {
                    // bullet i is fired at i * TimeToFireAll / NumFiredBullets seconds
                    if (t.ElapsedSeconds < TimeToFireAll * i / NumFiredBullets)
                        return;

                    GameplayManager.Instantiate(new Bullet(Tank.Position + Tank.LastDirection * Tank.SPAWN_DISTANCE_FROM_TANK, Tank.LastDirection, Tank));
                    BulletShot?.Invoke();
                    amountFired++;
                }
            }));
        }
    }
}
