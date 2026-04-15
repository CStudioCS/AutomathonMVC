using Automathon.Game.AbilitySystem;
using System;

namespace Automathon.Game.ShieldSystem
{
    public class Shield : Ability
    {
        private const int SHIELD_COOLDOWN_MILLIS = 3000;
        public Shield(Func<bool> shouldActivateParam) : base(cooldown: SHIELD_COOLDOWN_MILLIS, shouldActivate: shouldActivateParam)
        { }

        protected override void Activate()
        {
            Debug.Log("Shield activated!");
        }
    }
}