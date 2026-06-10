using Automathon.Engine;
using Automathon.Game.MapSystem;

namespace Automathon.Game
{
    public class WallData : EntityData, IEntityFactory
    {
        public Vector2Int Size;

        public WallData(Vector2Int position, Vector2Int size, int rotationMilli)
            : base(position, rotationMilli)
        {
            Size = size;
        }

        public Entity ToEntity() => new Wall(this);
    }
}
