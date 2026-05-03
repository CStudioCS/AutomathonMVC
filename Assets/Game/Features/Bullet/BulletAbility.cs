using Automathon.Engine;
using Automathon.Game.AbilitySystem;
using Automathon.Game.TankSystem;
using System;

namespace Automathon.Game.BulletSystem
{
    public class BulletAbility : Ability
    {
        private const int COOLDOWN_MILLIS = 300;
        private const int SPAWN_DISTANCE_FROM_TANK = 1000;

        public BulletAbility(Func<bool> shouldActivate) : base(cooldown: COOLDOWN_MILLIS, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * SPAWN_DISTANCE_FROM_TANK / 1000;
            GameplayManager.Instantiate(new Bullet(position, Tank.LastMilliDirection));
        }
    }
}