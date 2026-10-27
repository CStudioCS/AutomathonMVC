using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class GrenadeAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 0.2; // seconds

        public GrenadeAbility(Func<bool> shouldActivate) : base(cooldown: COOLDOWN, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            FixedVector2 position = Tank.Position + Tank.LastDirection * Tank.SPAWN_DISTANCE_FROM_TANK;
            GameplayManager.Instantiate(new Grenade(position, Tank.LastDirection));
        }
    }
}
