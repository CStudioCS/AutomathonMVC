using Automathon.Game.World;
using UnityEngine;

namespace Automathon.Utility
{
    public static class ViewMath
    {
        public static Quaternion MilliRadRotationToQuaternion(float rotationMillirad)
        {
            return Quaternion.Euler(0, 0, rotationMillirad * Mathf.Rad2Deg / 1000f);
        }

        public static Vector2Int ToVector2IntScaled(this Vector2 vector)
            => new Vector2Int((int)(vector.x * WorldConstants.SPACE_SCALE), (int)(vector.y * WorldConstants.SPACE_SCALE));

        public static Vector3 ScreenToWorldSpace(this Vector2 screenPos)
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -1));
        }
    }
}
