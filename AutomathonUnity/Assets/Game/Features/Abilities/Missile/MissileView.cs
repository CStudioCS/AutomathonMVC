using UnityEngine;
using UnityEngine.VFX;

namespace Automathon.Game
{
    public class MissileView : EntityView<Missile>
    {
        [SerializeField] private float cameraShakeIntensity;
        [SerializeField] private float cameraShakeDuration;
        [SerializeField] private VisualEffect BigExplosion;
        protected override void OnControllerDestroyed()
        {
            VisualEffect bigExplosion = Instantiate(BigExplosion, transform.position, Quaternion.identity);
            bigExplosion.SetFloat("Radius", Missile.AOE_RADIUS / 1000);

            if (Camera.main != null)
                Camera.main.GetComponent<CameraShaker>().CameraShake(cameraShakeDuration, cameraShakeIntensity);

            base.OnControllerDestroyed();
        }
    }
}