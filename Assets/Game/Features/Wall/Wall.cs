using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.MapSystem;

namespace Automathon.Game.WallSystem
{
    public class Wall : Entity, IPersistable
    {
        public WallData WallData { get; }
        public Vector2Int HalfSize => WallData.HalfSize;
        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public Wall(Vector2Int position, Vector2Int halfSize, int rotationMilli)
            : this(new WallData(position, halfSize, rotationMilli)) { }

        public Wall(WallData data) : base(data)
        {
            WallData = data;
            BoxCollider = new BoxCollider(Vector2Int.Zero, data.HalfSize.X, data.HalfSize.Y, 0);
            Rigidbody = new Rigidbody(BoxCollider, 0, 0, 200);

            Initialize(BoxCollider, Rigidbody);
        }

        public EntityData ToData() => WallData;
    }
}
