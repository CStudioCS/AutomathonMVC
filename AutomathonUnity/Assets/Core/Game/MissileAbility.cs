using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class MissileAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 1.5; // seconds
        private static readonly Fixed SPAWN_DISTANCE_FROM_TANK = 1.5;

        public MissileAbility(Func<bool> shouldActivate) : base(COOLDOWN, shouldActivate)
        { }

        protected override void Activate()
        {
            FixedVector2 position = Tank.Position + Tank.LastDirection * SPAWN_DISTANCE_FROM_TANK;
            GameplayManager.Instantiate(new Missile(position, Tank));
        }
    }
}
