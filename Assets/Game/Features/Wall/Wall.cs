using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.MapSystem;

namespace Automathon.Game.WallSystem
{
    public class Wall : Entity, IPersistable
    {
        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public Wall(Vector2Int position, Vector2Int halfSize, int rotationMilli) : base(position)
        {
            RotationMilli = rotationMilli;
            BoxCollider = new BoxCollider(Vector2Int.Zero, halfSize.X, halfSize.Y, 0);
            Rigidbody = new Rigidbody(BoxCollider, 0, 0, 200);

            Initialize(BoxCollider, Rigidbody);
        }

        public EntityData ToData() => new WallData(
            Position,
            new Vector2Int(BoxCollider.Width / 2, BoxCollider.Height / 2),
            RotationMilli
        );
    }
}
