using Automathon.Engine;
using System;


namespace Automathon.Game.AbilitySystem
{
    public abstract class Ability : Component
    {
        private int coolDownMillis;
        private bool isOnCooldown = false;
        private Func<bool> shouldActivate;

        public event Action AbilityActivated;
        public event Action CooldownElapsed;

        public Ability(int cooldown, Func<bool> shouldActivate)
        {
            this.coolDownMillis = cooldown;
            this.shouldActivate = shouldActivate;
        }

        public override void Update()
        {
            base.Update();

            if (shouldActivate())
            {
                TryActivate();
            }
        }

        public void TryActivate()
        {
            if (isOnCooldown)
                return;

            isOnCooldown = true;
            ParentEntity.AddBehavior(new Timer(coolDownMillis, null, OnComplete: () =>
            {
                isOnCooldown = false;
                CooldownElapsed?.Invoke();
            }));
            Activate();
            AbilityActivated?.Invoke();
        }

        protected abstract void Activate();
    }
}