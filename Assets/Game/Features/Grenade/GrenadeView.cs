using Automathon.Game.View;
using Automathon.Game.World;
using UnityEngine;



namespace Automathon.Game.GrenadeSystem
{
    public class GrenadeView : EntityView<Grenade>
    {
        public override void Initialize(Grenade grenade)
        {
            base.Initialize(grenade);
            transform.localScale = Vector3.one * 2 * grenade.CircleCollider.Radius / (float)WorldConstants.SPACE_SCALE;
            grenade.OnBlowedUp += OnBlowUp;
        }

        private void OnBlowUp()
        {
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if (Entity != null)
            {
                Entity.OnBlowedUp -= OnBlowUp;
            }
        }
    }

}