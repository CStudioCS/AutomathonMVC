using UnityEngine;

namespace Automathon.Utility
{
    public static class ViewMath
    {
        public static Quaternion MilliRadRotationToQuaternion(float rotationMillirad)
        {
            return Quaternion.Euler(0, 0, rotationMillirad * Mathf.Rad2Deg / 1000f);
        }
    }
}
