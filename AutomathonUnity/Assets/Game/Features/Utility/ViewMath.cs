using UnityEngine;

namespace Automathon.Game.View
{
    public static class ViewMath
    {
        /// <summary>Rotation around Z from an angle in radians (the simulation's convention).</summary>
        public static Quaternion RadRotationToQuaternion(float rotationRad)
            => Quaternion.Euler(0, 0, rotationRad * Mathf.Rad2Deg);

        public static Quaternion RadRotationToQuaternion(Fixed rotationRad)
            => RadRotationToQuaternion((float)rotationRad);

        /// <summary>Converts a Unity vector (input, world position...) to the simulation's fixed point space.</summary>
        public static FixedVector2 ToFixedVector2(this Vector2 vector)
            => new FixedVector2(vector.x / WorldConstants.SPACE_SCALE, vector.y / WorldConstants.SPACE_SCALE);

        public static Vector3 ScreenToWorldSpace(this Vector2 screenPos)
            => Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

        public static float TriangleFunction(float period, float amplitude = 1f)
        {
            float tmp = Time.time / period;
            return (amplitude * (1f - 2f * Mathf.Abs((tmp - Mathf.Floor(tmp)) - (1f / 2f))));
        }
    }
}
