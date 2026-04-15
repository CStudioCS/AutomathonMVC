using Automathon.Game.World;
using UnityEngine;

namespace Automathon.Game.Utility
{
    public static class VectorIntExtensions
    {
        public static Vector2 ToVector2Scaled(this Vector2Int v)
            => new Vector2(v.X, v.Y) / WorldConstants.SPACE_SCALE;
    }
}
