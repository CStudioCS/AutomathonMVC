using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game
{
    public class Bullet : Entity
    {
        public class BulletState : State
        {
            public Fixed Radius;
            public FixedVector2 Velocity;
        }

        private static readonly Fixed DAMAGE = 100;
        public static readonly Fixed RADIUS = 0.1;
        /// <summary>Speed in units per second.</summary>
        public static readonly Fixed SPEED = 17;
        private static readonly Fixed LIFESPAN = 10; // seconds

        private Tank shotFromTank;

        private Rigidbody rigidbody;
        private CircleCollider circleCollider;

        public Action HitWall;
        public Action HitTank;

        public Bullet(FixedVector2 position, FixedVector2 direction, Tank shotFrom) : base(position)
        {
            this.shotFromTank = shotFrom;

            direction = direction.Normalized();

            circleCollider = new CircleCollider(FixedVector2.Zero, RADIUS);
            circleCollider.Layer = CollisionLayer.Bullet;
            rigidbody = new Rigidbody(circleCollider, invMass: 10, invInertia: 0.3, friction: 0);

            Initialize(circleCollider, rigidbody);

            rigidbody.Velocity = direction * SPEED;
            circleCollider.OnCollision += OnCollision;

            AddBehavior(new Timer(LIFESPAN, null, () => GameplayManager.Destroy(this)));
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity == shotFromTank)
                return;

            if (collisionContact.Other.ParentEntity is Wall wall)
            {
                wall.OnHit?.Invoke(collisionContact.Contacts[0].Position, 500);
                HitWall?.Invoke();
            }


            if (collisionContact.Other.Layer == CollisionLayer.Tank)
                HitTank?.Invoke();

            if (collisionContact.Other.ParentEntity.TryGetComponent(out Health health))
                health.Damage(DAMAGE);

            GameplayManager.Destroy(this);
        }

        public override void OnDestroyed()
        {
            //technically doing this isn't necessary since it's not a memory leak since circleCollider gets destroyed along with the entity
            //better safe than sound ig
            circleCollider.OnCollision -= OnCollision;
            base.OnDestroyed();
        }

        public override State GetState()
        {
            return new BulletState()
            {
                Position = this.Position,
                Radius = RADIUS,
                Velocity = rigidbody.Velocity,
            };
        }
    }
}
