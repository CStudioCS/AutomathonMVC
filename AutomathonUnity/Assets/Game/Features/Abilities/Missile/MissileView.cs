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
            // Missile colour is authored on the prefab's SpriteRenderer; its smoke-trail VFX is
            // disabled on the prefab (no trail behind the missile).
            SoundManager.instance.PlaySound("FireMissile");
        }

        protected override void OnControllerDestroyed()
        {
            SoundManager.instance.PlaySound("ExplosionMissile");
            ScreenGlitch.TriggerHeavy();

            VisualEffect bigExplosion = Instantiate(BigExplosion, transform.position, Quaternion.identity);
            bigExplosion.SetFloat("Radius", Missile.AOE_RADIUS / 1000);

            if (Camera.main != null)
                Camera.main.GetComponent<CameraShaker>().CameraShake(cameraShakeDuration, cameraShakeIntensity);

            // Leave a black scorch mark in the mud at the blast.
            Automathon.Game.View.MudLayerController.AddExplosionMark(transform.position, Missile.AOE_RADIUS / 1000f);

            base.OnControllerDestroyed();
        }
    }
}