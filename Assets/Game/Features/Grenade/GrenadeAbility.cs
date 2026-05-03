using Automathon.Engine;
using Automathon.Engine.Utility;
using Automathon.Game.AbilitySystem;
using Automathon.Game.TankSystem;
using System;

namespace Automathon.Game.GrenadeSystem
{
    public class GrenadeAbility : Ability
    {
        private const int COOLDOWN_MILLIS = 3000;
        private const int SPAWN_DISTANCE_FROM_TANK = 1000;

        public GrenadeAbility(Func<bool> shouldActivate) : base(cooldown: COOLDOWN_MILLIS, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * SPAWN_DISTANCE_FROM_TANK / 1000;
            GameplayManager.Instantiate(new Grenade(position, Tank.LastMilliDirection));
        }
    }
}