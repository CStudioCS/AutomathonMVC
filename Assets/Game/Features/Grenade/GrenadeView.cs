using Automathon.Game.BulletSystem;
using Automathon.Game.Utility;
using Automathon.Game.World;
using UnityEngine;



namespace Automathon.Game.GrenadeSystem
{
    public class GrenadeView : MonoBehaviour
    {
        private Grenade grenade;

        public void Initialize(Grenade grenade)
        {
            this.grenade = grenade;
            transform.localScale = Vector3.one * 2 * grenade.CircleCollider.Radius / (float)WorldConstants.SPACE_SCALE;
            grenade.Blowed += OnBlow;
        }

        private void LateUpdate()
        {
            transform.position = grenade.Position.ToVector2Scaled();
        }

        private void OnBlow()
        {
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if(grenade != null)
            {
                grenade.Blowed -= OnBlow;
            }
        }
    }

}