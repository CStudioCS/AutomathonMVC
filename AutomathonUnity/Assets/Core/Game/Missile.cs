using Automathon.Engine;
using Automathon.Engine.Physics;
using System.Collections.Generic;
using System.Linq;

namespace Automathon.Game
{
    public class Missile : Entity
    {
        public class MissileState : State
        {
            public Fixed Radius;
            public Fixed Rotation;
            public FixedVector2 Velocity;
        }

        private const bool AIMED = true; //do we aim the missile with the right stick ?

        private static readonly Fixed DAMAGE = 400;
        private static readonly Fixed DIRECT_DAMAGE = 600;

        public static readonly Fixed RADIUS = 0.3;
        /// <summary>Speed in units per second.</summary>
        public static readonly Fixed SPEED = 9;
        private static readonly Fixed LIFESPAN = 5; // seconds
        /// <summary>Max rotation per frame (radians) when steering towards the aiming direction.</summary>
        private static readonly Fixed ROTATION_MAX_STEP = 0.04;

        public static readonly Fixed AOE_RADIUS = 1;

        private Tank shotFromTank;
        private CircleCollider circleCollider;
        private CircleCollider aoeCollider;
        private Rigidbody rigidbody;

        private bool initCollidingWithTank;
        private List<Entity> oldAoeCollisions = new();
        private List<Entity> aoeCollisions = new();

        public Missile(FixedVector2 position, Tank shotFromTank) : base(position)
        {
            this.shotFromTank = shotFromTank;

            circleCollider = new CircleCollider(FixedVector2.Zero, RADIUS);
            circleCollider.Layer = CollisionLayer.Missile;
            aoeCollider = new CircleCollider(FixedVector2.Zero, AOE_RADIUS);
            aoeCollider.IsTrigger = true;

            rigidbody = new Rigidbody(new CompositeCollider(circleCollider, aoeCollider), invMass: 3, invInertia: 0.3, friction: 0);

            circleCollider.OnCollision += OnCollision;
            aoeCollider.OnCollision += OnAoeCollision;

            Initialize(circleCollider, aoeCollider, rigidbody);

            Rotation = shotFromTank.LastDirection.Angle();

            if (shotFromTank.TryGetComponent<Collider>(out Collider coll))
                initCollidingWithTank = coll.Colliding(circleCollider);

            rigidbody.Velocity = FixedVector2.FromAngle(Rotation) * SPEED;

            AddBehavior(new Timer(LIFESPAN, null, () => GameplayManager.Destroy(this)));
        }

        public override void Update()
        {
            base.Update();

            oldAoeCollisions = aoeCollisions;
            aoeCollisions = new();

            if (initCollidingWithTank && shotFromTank.TryGetComponent<Collider>(out Collider coll) && !coll.Colliding(circleCollider))
                initCollidingWithTank = false;

            if (AIMED)
            {
                Fixed goalRotation = shotFromTank.LastDirection.Angle();
                Fixed step = FixedMath.DeltaAngle(Rotation, goalRotation);

                Rotation += FixedMath.Clamp(step, -ROTATION_MAX_STEP, ROTATION_MAX_STEP);
            }

            rigidbody.Velocity = FixedVector2.FromAngle(Rotation) * SPEED;
        }

        private void OnAoeCollision(CollisionEvent collisionContact)
        {
            aoeCollisions.Add(collisionContact.Other.ParentEntity);
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity is Wall wall)
                wall.OnHit?.Invoke(collisionContact.Contacts[0].Position, 5000);

            if (initCollidingWithTank && collisionContact.Other.ParentEntity == shotFromTank)
                return; //we want the missile to be able to collide with the original player

            if (collisionContact.Other.ParentEntity.TryGetComponent(out Health directHealth))
                directHealth.Damage(DIRECT_DAMAGE);

            foreach (Entity entity in aoeCollisions.Concat(oldAoeCollisions))
            {
                if (entity.TryGetComponent(out Health health))
                {
                    if (entity is Shield)
                        health.Kill();
                    else if (entity != collisionContact.Other.ParentEntity)
                        health.Damage(DAMAGE);
                }
            }

            GameplayManager.Destroy(this);
        }

        public override void OnDestroyed()
        {
            //technically doing this isn't necessary since it's not a memory leak since circleCollider gets destroyed along with the entity
            //better safe than sound ig
            circleCollider.OnCollision -= OnCollision;
            aoeCollider.OnCollision -= OnAoeCollision;
            base.OnDestroyed();
        }

        public override State GetState()
        {
            return new MissileState()
            {
                Position = this.Position,
                Radius = RADIUS,
                Rotation = this.Rotation,
                Velocity = rigidbody.Velocity,
            };
        }
    }
}
