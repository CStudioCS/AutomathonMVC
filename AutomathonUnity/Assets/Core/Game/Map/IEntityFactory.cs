using Automathon.Engine;

namespace Automathon.Game.MapSystem
{
    /// <summary>
    /// Implemented by an <see cref="EntityData"/> that knows how to rebuild its matching <see cref="Entity"/>.
    /// Symmetric counterpart of <see cref="IPersistable"/> on the entity side.
    /// </summary>
    public interface IEntityFactory
    {
        Entity ToEntity();
    }
}
