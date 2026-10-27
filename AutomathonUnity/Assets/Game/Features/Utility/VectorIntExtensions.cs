using UnityEngine;

namespace Automathon.Game
{
    /// <summary>Conversions from the simulation's fixed point types to Unity's float types (view side only).</summary>
    public static class VectorIntExtensions
    {
        public static Vector2 ToVector2(this FixedVector2 v)
            => new Vector2((float)v.X, (float)v.Y) * WorldConstants.SPACE_SCALE;

        public static Vector3 ToVector3(this FixedVector2 v, float z = 0f)
            => new Vector3((float)v.X * WorldConstants.SPACE_SCALE, (float)v.Y * WorldConstants.SPACE_SCALE, z);

        /// <summary>Converts a simulation length (Fixed) to Unity units.</summary>
        public static float ToWorldLength(this Fixed length)
            => (float)length * WorldConstants.SPACE_SCALE;
    }
}
