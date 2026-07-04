using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Engine.Utility;
using Automathon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automathon.Game
{
    public class Missile : Entity
    {
        public class MissileState : State
        {
            public int Radius;
            public Vector2Int Velocity;
        }

        private const bool AIMED = true; //do we aim the missile with the right stick ?

        private const int DAMAGE = 400;
        private const int DIRECT_DAMAGE = 600;

        public const int RADIUS = 300;
        public const int SPEED = 9000;
        private const int LIFESPAN_MILLI = 5000;
        private const int ROTATION_MAX_STEP = 40;

        public const int AOE_RADIUS = 1000;

        private Tank shotFromTank;
        private CircleCollider circleCollider;
        private CircleCollider aoeCollider;
        private Rigidbody rigidbody;

        private bool initCollidingWithTank;
        private List<Entity> oldAoeCollisions = new();
        private List<Entity> aoeCollisions = new();

        public Missile(Vector2Int position, Tank shotFromTank) : base(position)
        {
            this.shotFromTank = shotFromTank;

            circleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);
            circleCollider.Layer = CollisionLayer.Missile;
            aoeCollider = new CircleCollider(Vector2Int.Zero, AOE_RADIUS);
            aoeCollider.IsTrigger = true;

            rigidbody = new Rigidbody(new CompositeCollider(circleCollider, aoeCollider), 3000, 300, 200);

            circleCollider.OnCollision += OnCollision;
            aoeCollider.OnCollision += OnAoeCollision;

            Initialize(circleCollider, aoeCollider, rigidbody);


            RotationMilli = Atan2Int.Atan2(shotFromTank.LastMilliDirection.X, shotFromTank.LastMilliDirection.Y);

            if (shotFromTank.TryGetComponent<Collider>(out Collider coll))
                initCollidingWithTank = coll.Colliding(circleCollider);

            rigidbody.Velocity = new Vector2Int(TrigTable.Cos(RotationMilli), TrigTable.Sin(RotationMilli)) * SPEED / 1000;

            AddBehavior(new Timer(LIFESPAN_MILLI, null, () => GameplayManager.Destroy(this)));
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
                int goalRotationMilli = Atan2Int.Atan2(shotFromTank.LastMilliDirection.X, shotFromTank.LastMilliDirection.Y);
                int step = goalRotationMilli - RotationMilli;

                if (step > IntMath.PI_MILLI)
                    step -= IntMath.PI_MILLI * 2;

                if (step < -IntMath.PI_MILLI)
                    step += IntMath.PI_MILLI * 2;

                RotationMilli += Math.Clamp(step, -ROTATION_MAX_STEP, ROTATION_MAX_STEP);
            }

            rigidbody.Velocity = new Vector2Int(TrigTable.Cos(RotationMilli), TrigTable.Sin(RotationMilli)) * SPEED / 1000;
        }

        private void OnAoeCollision(CollisionEvent collisionContact)
        {
            aoeCollisions.Add(collisionContact.Other.ParentEntity);
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
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
                Velocity = rigidbody.Velocity,
            };
        }
    }
}
