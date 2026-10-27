using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class ShieldAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 2; // seconds
        private static readonly Fixed SPAWN_DISTANCE_FROM_TANK = 2;

        public ShieldAbility(Func<bool> shouldActivate) : base(COOLDOWN, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            FixedVector2 direction = Tank.LastDirection.Normalized();

            FixedVector2 position = Tank.Position + direction * SPAWN_DISTANCE_FROM_TANK;
            Fixed rotation = direction.Angle() + Fixed.HalfPi;

            GameplayManager.Instantiate(new Shield(position, rotation));
        }
    }
}
