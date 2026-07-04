using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class Health : Component
    {
        public readonly int MaxHealth;
        private readonly Action onDeath;
        private readonly bool destroyOnDeath;
        private bool invincible;

        public int CurrentHealth { get; private set; }

        public Health(int maxHealth, bool destroyOnDeath = true, Action onDeath = null)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            this.onDeath = onDeath;
            this.destroyOnDeath = destroyOnDeath;
            this.invincible = false;
        }

        public void MakeInvincible()
        {
            invincible = true;
        }

        public void MakeVulnerable()
        {
            invincible = false;
        }

        public void Damage(int damage)
        {
            if(!invincible)
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

        public void Kill()
            => Damage(MaxHealth);
    }
}
