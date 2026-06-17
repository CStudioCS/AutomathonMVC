using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game
{
    public class Missile : Entity
    {
        private const int DAMAGE = 400;
        public const int RADIUS = 300;
        public const int SPEED = 13000;
        private const int LIFESPAN_MILLI = 12000;

        private Tank shotFrom;
        private CircleCollider circleCollider;

        public Missile(Vector2Int position, Vector2Int direction, Tank shotFromTank) : base(position)
        {
            this.shotFrom = shotFromTank;

            circleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);
            Rigidbody rigidbody = new Rigidbody(circleCollider, 3000, 300, 200);

            Initialize(circleCollider, rigidbody);

            rigidbody.Velocity = direction * SPEED / 1000;
            circleCollider.OnCollision += OnCollision;

            AddBehavior(new Timer(LIFESPAN_MILLI, null, () => GameplayManager.Destroy(this)));
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity == shotFrom)
                return;

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
    }
}
