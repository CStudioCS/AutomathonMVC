using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.TankSystem;

namespace Automathon.Game.BulletSystem
{
    public class Bullet : Entity
    {
        public const int RADIUS = 200;
        public const int SPEED = 7000;
        private const int LIFESPAN_MILLI = 10000;

        private CircleCollider circleCollider;

        public Bullet(Vector2Int position, Vector2Int direction) : base(position)
        {
            circleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);

            Rigidbody rigidbody = new Rigidbody(circleCollider, 10000, 300, 200);
            rigidbody.Velocity = direction * SPEED / 1000;

            Initialize(circleCollider, rigidbody);

            circleCollider.OnCollision += OnCollision;

            AddBehavior(new Timer(LIFESPAN_MILLI, null, () => GameplayManager.Destroy(this)));
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity is Tank tank)
                tank.Damage(100);

            GameplayManager.Destroy(this);
        }

        public override void OnDestroyed()
        {
            //technically doing this isn't necessary since it's not a memory leak since circleCollider gets destroyed along with the entity
            //better safe than sound ig
            circleCollider.OnCollision -= OnCollision;
            base.OnDestroyed();
        }
    }
}
