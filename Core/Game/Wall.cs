using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.MapSystem;

namespace Automathon.Game
{
    public class Wall : Entity, IPersistable
    {
        public WallData WallData { get; }
        public Vector2Int Size => WallData.Size;
        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public Wall(Vector2Int position, Vector2Int size, int rotationMilli)
            : this(new WallData(position, size, rotationMilli)) { }

        public Wall(WallData data) : base(data)
        {
            WallData = data;
            BoxCollider = new BoxCollider(Vector2Int.Zero, data.Size.X, data.Size.Y, 0);
            this.BoxCollider.Layer = CollisionLayer.Wall;
            Rigidbody = new Rigidbody(BoxCollider, 0, 0, 200);

            Initialize(BoxCollider, Rigidbody);
        }

        public EntityData ToData() => WallData;
    }
}
