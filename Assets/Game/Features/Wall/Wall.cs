using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.BulletSystem;
using System;

namespace Automathon.Game.WallSystem
{
    public class Wall : Entity
    {
        public static event Action<Wall> Spawned;

        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public Wall(Vector2Int position, Vector2Int halfSize, int rotation) : base(position)
        {
            BoxCollider = new BoxCollider(position, halfSize.X, halfSize.Y, rotation);
            Rigidbody = new Rigidbody(BoxCollider);

            Initialize(BoxCollider, Rigidbody);

            Spawned?.Invoke(this);
        }
    }
}
