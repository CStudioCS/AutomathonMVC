using Automathon.Engine;
using Automathon.Game.MapSystem;

namespace Automathon.Game.WallSystem
{
    public class WallData : EntityData, IEntityFactory
    {
        public Vector2Int HalfSize;

        public WallData(Vector2Int position, Vector2Int halfSize, int rotationMilli)
            : base(position, rotationMilli)
        {
            HalfSize = halfSize;
        }

        public Entity ToEntity() => new Wall(this);
    }
}
