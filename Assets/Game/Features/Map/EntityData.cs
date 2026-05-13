using Automathon.Engine;

namespace Automathon.Game.MapSystem
{
    public abstract class EntityData
    {
        public Vector2Int Position;
        public int RotationMilli;

        protected EntityData(Vector2Int position, int rotationMilli)
        {
            Position = position;
            RotationMilli = rotationMilli;
        }

        public abstract Entity ToEntity();
    }
}
