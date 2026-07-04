namespace Automathon.Engine
{
    /// <summary>
    /// Intrinsic data of an <see cref="Entity"/> (position, rotation, and any extra fields added by subclasses).
    /// Owned by the entity itself — Position and RotationMilli on Entity are read/written through this object.
    /// Subclasses (e.g. WallData) add entity-specific fields and may implement IEntityFactory to be reconstructable.
    /// </summary>
    public class EntityData
    {
        public Vector2Int Position;
        public int RotationMilli;

        public EntityData(Vector2Int position, int rotationMilli = 0)
        {
            Position = position;
            RotationMilli = rotationMilli;
        }
    }
}
