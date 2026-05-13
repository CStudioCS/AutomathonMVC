using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.HealthSystem;
using Automathon.Game.TankSystem;

namespace Automathon.Game.BulletSystem
{
    public class Bullet : Entity
    {
        private const int DAMAGE = 100;
        public const int RADIUS = 100;
        public const int SPEED = 7000;
        private const int LIFESPAN_MILLI = 10000;
        private Tank parentShooter;

        private CircleCollider circleCollider;

        public Bullet(Vector2Int position, Vector2Int direction, Tank shooter) : base(position)
        {
            parentShooter = shooter;
            circleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);
            Rigidbody rigidbody = new Rigidbody(circleCollider, 10000, 300, 200);

            Initialize(circleCollider, rigidbody);

            rigidbody.Velocity = direction * SPEED / 1000;
            circleCollider.OnCollision += OnCollision;

            AddBehavior(new Timer(LIFESPAN_MILLI, null, () => GameplayManager.Destroy(this)));
        }

        private void OnCollision(CollisionEvent collisionContact)
        {
            if (collisionContact.Other.ParentEntity == parentShooter || (collisionContact.Other.ParentEntity is Bullet bullet && bullet.parentShooter == this.parentShooter))
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
