using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public abstract class Ability : Component
    {
        public int FramesOfCooldownLeft;
        /// <summary>Cooldown duration in seconds.</summary>
        public Fixed Cooldown { get; private set; }
        private bool isOnCooldown = false;
        private Func<bool> shouldActivate;

        public event Action AbilityActivated;
        public event Action CooldownElapsed;

        public Tank Tank { get; private set; }

        /// <summary>Cooldown left before the ability can be used again, in seconds.</summary>
        public Fixed CooldownLeft => GameplayConstants.FramesToSeconds(FramesOfCooldownLeft);

        public Ability(Fixed cooldown, Func<bool> shouldActivate)
        {
            this.Cooldown = cooldown;
            this.shouldActivate = shouldActivate;
        }

        public override void Initialize(Entity parentEntity)
        {
            base.Initialize(parentEntity);

            if (ParentEntity is Tank tank)
                Tank = tank;
            else
                throw new ArgumentException("Abilities can only be added to a Tank");
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
            ParentEntity.AddBehavior(new Timer(Cooldown, (t) => FramesOfCooldownLeft = t.ValueFrames, OnComplete: () =>
            {
                isOnCooldown = false;
                CooldownElapsed?.Invoke();
                FramesOfCooldownLeft = 0;
            }));

            Activate();
            AbilityActivated?.Invoke();
        }

        protected abstract void Activate();
    }
}