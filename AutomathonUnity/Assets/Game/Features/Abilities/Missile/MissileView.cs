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
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color32(255, 68, 204, 255); // #ff44cc pink (body sprite)

            // No trail behind the missile — disable the smoke VFX (it acts as a trail).
            foreach (VisualEffect vfx in GetComponentsInChildren<VisualEffect>())
                vfx.enabled = false;

            SoundManager.instance.PlaySound("FireMissile");
        }

        protected override void OnControllerDestroyed()
        {
            SoundManager.instance.PlaySound("ExplosionMissile");

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