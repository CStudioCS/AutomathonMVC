using Automathon.Engine;
using Automathon.Engine.Utility;
using Automathon.Game.AbilitySystem;
using Automathon.Game.TankSystem;
using System;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldAbility : Ability
    {
        private const int SHIELD_COOLDOWN_MILLIS = 3000;
        private const int SPAWN_DISTANCE_FROM_TANK = 2000;
        public Tank Tank { get; private set; }

        public ShieldAbility(Tank tank, Func<bool> shouldActivateParam) : base(cooldown: SHIELD_COOLDOWN_MILLIS, shouldActivate: shouldActivateParam)
        {
            Tank = tank;
        }

        protected override void Activate()
        {
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * SPAWN_DISTANCE_FROM_TANK / 1000;
            int rotationMilliRad = Tank.BoxCollider.RotationMillirad + IntMath.PI_MILLI / 2;

            GameplayManager.Instantiate(new Shield(position, rotationMilliRad));
        }
    }
}