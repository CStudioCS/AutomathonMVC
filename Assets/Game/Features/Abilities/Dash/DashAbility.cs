using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game
{
    public class DashAbility : Ability
    {
        private const int COOLDOWN_MILLIS = 700;
        private const int DASH_SPEED_MILLI = 25000; // milli-units per frame
        public const int DASH_DURATION_MILLIS = 150;
        private Rigidbody rigidbody;
        private Vector2Int originalVelocity;

        public DashAbility(Func<bool> shouldActivate) : base(cooldownMilli: COOLDOWN_MILLIS, shouldActivate: shouldActivate)
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
            Vector2Int dashDirection = Tank.LastMovingMilliDirection;
            dashDirection.NormalizeAtScale(1000);

            // Store original velocity and apply dash velocity
            originalVelocity = rigidbody.Velocity;
            rigidbody.Velocity = (dashDirection * DASH_SPEED_MILLI) / 1000;

            // Mark tank as dashing
            if (Tank != null)
                Tank.IsDashing = true;

            // Start the dash effect timer
            ParentEntity.AddBehavior(new Timer(DASH_DURATION_MILLIS, null, OnComplete: () =>
            {
                // Restore original velocity
                rigidbody.Velocity = originalVelocity;

                // Mark tank as no longer dashing
                if (Tank != null)
                    Tank.IsDashing = false;
            }));
        }
    }
}
