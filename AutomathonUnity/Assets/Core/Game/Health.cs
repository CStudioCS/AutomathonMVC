using Automathon.Engine;
using System;

namespace Automathon.Game
{
    public class Health : Component
    {
        public readonly Fixed MaxHealth;
        private readonly Action onDeath;
        private readonly bool destroyOnDeath;
        private bool invincible;

        public Fixed CurrentHealth { get; private set; }

        /// <summary>CurrentHealth / MaxHealth, in [0, 1].</summary>
        public Fixed Ratio => MaxHealth > 0 ? CurrentHealth / MaxHealth : Fixed.Zero;

        public bool IsDead => CurrentHealth <= 0;

        public Health(Fixed maxHealth, bool destroyOnDeath = true, Action onDeath = null)
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

        public void Damage(Fixed damage)
        {
            if (!invincible)
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
