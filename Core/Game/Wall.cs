using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game
{
    public class Wall : Entity
    {
        public class WallState : State
        {
            public required Vector2Int Size;
            public required float RotationMilli;
        }

        public Vector2Int Size;
        private float rotationMilli;
        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public Wall(Vector2Int position, Vector2Int size, int rotationMilli) : base(position)
        {
            this.rotationMilli = rotationMilli;
            BoxCollider = new BoxCollider(Vector2Int.Zero, size.X, size.Y, 0);
            this.BoxCollider.Layer = CollisionLayer.Wall;
            Rigidbody = new Rigidbody(BoxCollider, 0, 0, 200);

            Initialize(BoxCollider, Rigidbody);
        }

        public override State GetState()
            => new WallState() { Position = this.Position, RotationMilli = rotationMilli, Size = this.Size };
    }
}
