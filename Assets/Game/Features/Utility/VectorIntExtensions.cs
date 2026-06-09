using UnityEngine;

namespace Automathon.Game
{
    public static class VectorIntExtensions
    {
        public static Vector2 ToVector2Scaled(this Vector2Int v)
            => new Vector2(v.X, v.Y) / WorldConstants.SPACE_SCALE;
    }
}
