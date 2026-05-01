using Automathon.Engine;
using Automathon.Engine.Physics;
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
            BoxCollider = new BoxCollider(Vector2Int.Zero, halfSize.X, halfSize.Y, rotation);
            Rigidbody = new Rigidbody(BoxCollider, 0, 0, 200);

            Initialize(BoxCollider, Rigidbody);

            Spawned?.Invoke(this);
        }
    }
}
