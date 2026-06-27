using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class BulletAbility : Ability
    {
        private const int COOLDOWN_MILLIS = 300;

        public BulletAbility(Func<bool> shouldActivate) : base(cooldownMilli: COOLDOWN_MILLIS, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * Tank.SPAWN_DISTANCE_FROM_TANK / 1000;
            GameplayManager.Instantiate(new Bullet(position, Tank.LastMilliDirection, Tank));
        }
    }
}