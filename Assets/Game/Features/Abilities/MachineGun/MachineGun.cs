using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class MachineGun : Ability
    {
        public int NumFiredBullets;
        public int TimeToFireAllMilli;

        public MachineGun(int numFiredBullets, int timeToFireAllMilli, int cooldown, Func<bool> shouldActivate) : base(cooldown, shouldActivate)
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
                    if (t.MaxValueFrames - t.ValueFrames * GameplayConstants.FRAMERATE < i * TimeToFireAllMilli / NumFiredBullets)
                        break;

                    GameplayManager.Instantiate(new Bullet(Tank.Position, Tank.LastMilliDirection));
                }
            }));
        }
    }
}
