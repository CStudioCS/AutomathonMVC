using Automathon.Engine;
using System;

namespace Automathon.Game.HealthSystem
{
    public class Health : Component
    {
        public readonly int MaxHealth;
        private readonly Action onDeath;
        private readonly bool destroyOnDeath;

        public int CurrentHealth { get; private set; }

        public Health(int maxHealth, bool destroyOnDeath = true, Action onDeath = null)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            this.onDeath = onDeath;
            this.destroyOnDeath = destroyOnDeath;
        }

        public void Damage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                onDeath?.Invoke();

                if (destroyOnDeath)
                    GameplayManager.Destroy(ParentEntity);
            }
        }
    }
}
