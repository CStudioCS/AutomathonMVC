using Automathon.Game.Utility;
using Automathon.Utility;
using UnityEngine;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldView : MonoBehaviour
    {
        private Shield shield;

        internal void Initialize(Shield shield)
        {
            this.shield = shield;
        }

        private void LateUpdate()
        {
            transform.position = shield.Position.ToVector2Scaled();
            transform.rotation = ViewMath.MilliRadRotationToQuaternion(shield.BoxCollider.RotationMillirad);
        }
    }
}

