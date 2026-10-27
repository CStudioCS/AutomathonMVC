using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game
{
    public class DashAbility : Ability
    {
        private static readonly Fixed COOLDOWN = 0.7;           // seconds
        private static readonly Fixed DASH_SPEED = 25;          // units per second
        public static readonly Fixed DASH_DURATION = 0.15;      // seconds
        private Rigidbody rigidbody;
        private FixedVector2 originalVelocity;

        public DashAbility(Func<bool> shouldActivate) : base(cooldown: COOLDOWN, shouldActivate: shouldActivate)
        { }

        public override void Initialize(Entity parentEntity)
        {
            base.Initialize(parentEntity);

            // Get and store the rigidbody reference
            if (!ParentEntity.TryGetComponent<Rigidbody>(out rigidbody))
            {
                throw new ArgumentException("Tank must have a Rigidbody component for DashAbility");
            }
        }

        protected override void Activate()
        {
            FixedVector2 dashDirection = Tank.LastMovingDirection.Normalized();

            // Store original velocity and apply dash velocity
            originalVelocity = rigidbody.Velocity;
            rigidbody.Velocity = dashDirection * DASH_SPEED;

            // Mark tank as dashing
            if (Tank != null)
            {
                Tank.IsDashing = true;
                Tank.Health.MakeInvincible();
                Tank.Rigidbody.Collider.Layer = CollisionLayer.Dash;
            }

            // Start the dash effect timer
            ParentEntity.AddBehavior(new Timer(DASH_DURATION, null, OnComplete: () =>
            {
                // Restore original velocity
                rigidbody.Velocity = originalVelocity;

                // Mark tank as no longer dashing
                if (Tank != null)
                {
                    Tank.IsDashing = false;
                    Tank.Health.MakeVulnerable();
                    Tank.Rigidbody.Collider.Layer = CollisionLayer.Tank;
                }
                   
            }));
        }
    }
}
