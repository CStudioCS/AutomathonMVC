namespace Automathon.Engine
{
    /// <summary>
    /// Intrinsic data of an <see cref="Entity"/> (position, rotation, and any extra fields added by subclasses).
    /// Owned by the entity itself — Position and Rotation on Entity are read/written through this object.
    /// Subclasses (e.g. WallData) add entity-specific fields and may implement IEntityFactory to be reconstructable.
    /// Position is in world units (1 = 1 Unity unit), Rotation in radians.
    /// </summary>
    public class EntityData
    {
        public FixedVector2 Position;
        public Fixed Rotation;

        public EntityData(FixedVector2 position, Fixed rotation = default)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
