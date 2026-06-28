using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class MissileAbility : Ability
    {
        private const int COOLDOWN_MILLI = 1500;

        public MissileAbility(Func<bool> shouldActivate) : base(COOLDOWN_MILLI, shouldActivate)
        { }

        protected override void Activate()
        {
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * 1500 / 1000;
            GameplayManager.Instantiate(new Missile(position, Tank));
        }
    }
}
