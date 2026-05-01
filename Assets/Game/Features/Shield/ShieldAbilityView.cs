using Automathon.Game.Utility;
using Automathon.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldAbilityView : MonoBehaviour
    {
        private ShieldAbility shieldAbility;
        [SerializeField] private ShieldView shieldPrefab;
        private Dictionary<Shield, ShieldView> activeShields = new Dictionary<Shield, ShieldView>();

        public void Initialize(ShieldAbility shieldAbility)
        {
            this.shieldAbility = shieldAbility;
            this.shieldAbility.ShieldActivated += OnShieldActivated;
            this.shieldAbility.ShieldDestroyed += OnShieldDestroyed;
        }

        private void OnShieldActivated(Shield shield)
        {
            Vector2 shieldPosition = shieldAbility.Tank.Position.ToVector2Scaled();
            Quaternion shieldRotation = ViewMath.MilliRadRotationToQuaternion(shield.BoxCollider.RotationMillirad);
            ShieldView shieldInstance = Instantiate(shieldPrefab, shieldPosition, shieldRotation);
            shieldInstance.Initialize(shield);
            activeShields[shield] = shieldInstance;
        }

        private void OnShieldDestroyed(Shield shield)
        {
            Destroy(activeShields[shield].gameObject);
            activeShields.Remove(shield);
        }
    }
}
