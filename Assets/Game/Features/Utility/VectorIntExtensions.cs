using UnityEngine;

namespace Automathon.Game.Utility
{
    public static class VectorIntExtensions
    {
        public static Vector2Int ToV2Int(this Vector2 v)
            => new Vector2Int((int)v.x, (int)v.y);

        public static Vector2 ToV2(this Vector2Int v)
            => new Vector2(v.X, v.Y);
    }
}
