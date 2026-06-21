using UnityEngine;

namespace Automathon.Game
{
    public class GrenadeView : EntityView<Grenade>
    {
        [SerializeField] float explosionShakeTime;
        [SerializeField] float explosionShakeIntensity;

        public override void Initialize(Grenade entity)
        {
            base.Initialize(entity);
            Entity.Explode += OnExplode;
        }

        private void OnExplode()
        {
            Camera.main.GetComponent<CameraShaker>().CameraShake(explosionShakeTime, explosionShakeIntensity);
        }
    }

}