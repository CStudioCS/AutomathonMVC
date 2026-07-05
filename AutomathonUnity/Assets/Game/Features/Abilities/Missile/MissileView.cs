using UnityEngine;
using UnityEngine.VFX;

namespace Automathon.Game
{
    public class MissileView : EntityView<Missile>
    {
        [SerializeField] private float cameraShakeIntensity;
        [SerializeField] private float cameraShakeDuration;
        [SerializeField] private VisualEffect BigExplosion;

        public override void Initialize(Missile entity)
        {
            base.Initialize(entity);
            SoundManager.instance.PlaySound("FireMissile");
        }

        protected override void OnControllerDestroyed()
        {
            SoundManager.instance.PlaySound("ExplosionMissile");

            VisualEffect bigExplosion = Instantiate(BigExplosion, transform.position, Quaternion.identity);
            bigExplosion.SetFloat("Radius", Missile.AOE_RADIUS / 1000);

            if (Camera.main != null)
                Camera.main.GetComponent<CameraShaker>().CameraShake(cameraShakeDuration, cameraShakeIntensity);

            base.OnControllerDestroyed();
        }
    }
}