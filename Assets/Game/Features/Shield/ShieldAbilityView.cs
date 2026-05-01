using Automathon.Game.Utility;
using Automathon.Utility;
using UnityEngine;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldAbilityView : MonoBehaviour
    {
        private ShieldAbility shieldAbility;
        [SerializeField] private ShieldView shieldPrefab;
        //create a list of active shields ?

        public void Initialize(ShieldAbility shieldAbility)
        {
            this.shieldAbility = shieldAbility;
            this.shieldAbility.ShieldActivated += OnShieldActivated;
        }

        private void OnShieldActivated(Shield shield)
        {
            Vector2 shieldPosition = shieldAbility.Tank.Position.ToVector2Scaled();
            Quaternion shieldRotation = ViewMath.MilliRadRotationToQuaternion(shield.BoxCollider.RotationMillirad);
            ShieldView shieldInstance = Instantiate(shieldPrefab, shieldPosition, shieldRotation);
            shieldInstance.Initialize(shield);
        }
    }
}
