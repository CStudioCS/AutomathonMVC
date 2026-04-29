using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game.BulletSystem
{
    public class Bullet : Entity
    {
        public static event Action<Bullet> OnSpawn;
        public Rigidbody Rigidbody { get; private set; }

        public Bullet(Vector2Int position, Vector2Int direction, int speed, int radius) : base(position)
        {
            Collider coll = new CircleCollider(Vector2Int.Zero, radius);
            Rigidbody = new Rigidbody(coll);
            Rigidbody.Velocity = direction * speed / 1000;

            // Ensure components have their ParentEntity set
            Initialize(coll, Rigidbody);

            // Notify listeners after the bullet is fully initialized
            OnSpawn?.Invoke(this);
        }
    }
}


