using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Utility;
using System;

namespace Automathon.Game
{
    public class Bullet : Entity
    {
        public class BulletState : State
        {
            public int Radius;
            public Vector2Int Velocity;
        }

        private const int DAMAGE = 100;
        public const int RADIUS = 100;
        public const int SPEED = 17000;
        private const int LIFESPAN_MILLI = 10000;

        private Tank shotFromTank;

        private Rigidbody rigidbody;
        private CircleCollider circleCollider;

        public Action HitWall;
        public Action HitTank;

        public Bullet(Vector2Int position, Vector2Int direction, Tank shotFrom) : base(position)
        {
            this.shotFromTank = shotFrom;

            direction.NormalizeAtScale(1000);

            circleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);
            circleCollider.Layer = CollisionLayer.Bullet;
            rigidbody = new Rigidbody(circleCollider, 10000, 300, 200);

            Initialize(circleCollider, rigidbody);

            rigidbody.Velocity = direction * SPEED / 1000;
            circleCollider.OnCollision += OnCollision;

            AddBehavior(new Timer(LIFESPAN_MILLI, null, () => GameplayManager.Destroy(this)));
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity == shotFromTank)
                return;

            if (collisionContact.Other.Layer == CollisionLayer.Wall || collisionContact.Other.Layer == CollisionLayer.Shield)
            {
                Wall wall = (Wall)collisionContact.Other.ParentEntity;

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
