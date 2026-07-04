using Automathon.Engine;
using Automathon.Engine.Utility;
using System;

namespace Automathon.Game
{
    public class ShieldAbility : Ability
    {
        private const int COOLDOWN_MILLIS = 2000;
        private const int SPAWN_DISTANCE_FROM_TANK = 2000;

        public ShieldAbility(Func<bool> shouldActivate) : base(COOLDOWN_MILLIS, shouldActivate: shouldActivate)
        { }

        protected override void Activate()
        {
            Vector2Int direction = Tank.LastMilliDirection;
            direction.NormalizeAtScale(1000);

            Vector2Int position = Tank.Position + direction * SPAWN_DISTANCE_FROM_TANK / 1000;
            int rotationMilliRad = Atan2Int.Atan2(Tank.LastMilliDirection.X, Tank.LastMilliDirection.Y) + IntMath.PI_MILLI / 2;

            GameplayManager.Instantiate(new Shield(position, rotationMilliRad));
        }
    }
}