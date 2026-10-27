using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class BulletAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 0.3; // seconds

        public BulletAbility(Func<bool> shouldActivate) : base(cooldown: COOLDOWN, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            FixedVector2 position = Tank.Position + Tank.LastDirection * Tank.SPAWN_DISTANCE_FROM_TANK;
            GameplayManager.Instantiate(new Bullet(position, Tank.LastDirection, Tank));
        }
    }
}
