using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game.BulletSystem
{
    public class Bullet : Entity
    {
        public static event Action<Bullet> Spawned;
        public Rigidbody Rigidbody { get; private set; }
        public CircleCollider CircleCollider { get; private set; }

        public Bullet(Vector2Int position, Vector2Int direction, int speed, int radius) : base(position)
        {
            CircleCollider = new CircleCollider(Vector2Int.Zero, radius);
            Rigidbody = new Rigidbody(CircleCollider, 10000, 300, 200);
            Rigidbody.Velocity = direction * speed / 1000;

            Initialize(CircleCollider, Rigidbody);

            Spawned?.Invoke(this);
        }
    }
}


