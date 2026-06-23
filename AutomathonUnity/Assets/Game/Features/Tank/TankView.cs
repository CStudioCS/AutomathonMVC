using Assets.Game.View;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private float bulletShakingIntensity;
        [SerializeField] private float bulletShakingDuration;

        [SerializeField] private float bulletCameraShakingIntensity;
        [SerializeField] private float bulletCameraShakingDuration;

        [SerializeField] private float DashShakingIntensity;
        [SerializeField] private float DashCameraShakingIntensity;
        [SerializeField] private AudioSource DashA;

        [SerializeField] private Transform turret;
        [SerializeField] private Transform turretTip;
        [SerializeField] private Transform body;
        [SerializeField] private ParticleSystem dashBurstParticleSystem;
        [SerializeField] private ParticleSystem dashFlame;
        [SerializeField] private VisualEffect miniExplosion;

        private CameraShaker cameraShaker;

        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);

            //Entity.BulletAbility.AbilityActivated += OnShooting;
            //Entity.GrenadeAbility.AbilityActivated += OnGrenadeAbility;
            Entity.MachineGunAbility.BulletShot += OnMachineGunAbilityBulletShot;
            Entity.DashAbility.AbilityActivated += OnDashAbility;
            Entity.MissileAbility.AbilityActivated += OnMissileAbility;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }

        private void OnMachineGunAbilityBulletShot()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }

            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
            cameraShaker.CameraShake(bulletShakingDuration, bulletCameraShakingIntensity);

            EmitShootingMiniExplosion();
        }

        public void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        IEnumerator Dash(int dashDurationMili)
        {
            float dashDuration = (float)dashDurationMili / 1000f;

            SetAlpha(0.5f);

            dashFlame.Play();
            float timer = -0.10f;

            while (timer < dashDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            dashFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SetAlpha(1f);
        }

        private void OnDashAbility()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }
            StartCoroutine(Shaker.Translate(turret, new Vector2(0, -1), DashAbility.DASH_DURATION_MILLIS, DashShakingIntensity));
            cameraShaker.CameraTranslate(body.right, DashAbility.DASH_DURATION_MILLIS, DashCameraShakingIntensity);
            DashA.Play();
            //dashBurstParticleSystem.Play();  imo pas besoin de burst initiale mais bon c'est implémenté quoi
            StartCoroutine(Dash(DashAbility.DASH_DURATION_MILLIS));
        }

        private void OnGrenadeAbility()
        {
            //No shake when shooting a grenade imo
            //StartCoroutine(Shaker.Shake(turret, grenadeShakingDuration, grenadeShakingIntensity));
        }

        private void OnMissileAbility()
        {
            EmitShootingMiniExplosion();
        }

        protected override void OnDestroy()
        {
            UnSub();
            base.OnDestroy();
        }

        protected override void OnControllerDestroyed()
        {
            UnSub();
            base.OnControllerDestroyed();
        }

        private void EmitShootingMiniExplosion()
        {
            VisualEffect miniExplosionVFX = Instantiate(miniExplosion, turretTip.position, Quaternion.identity);
            miniExplosionVFX.SetVector2("InitVelocity", Entity.Rigidbody.Velocity.ToVector2Scaled());
            float rot = turret.rotation.eulerAngles.z * Mathf.Deg2Rad;
            miniExplosionVFX.SetVector2("Direction", new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)));
        }

        private void UnSub()
        {
            //Entity.BulletAbility.AbilityActivated -= OnShooting;
            //Entity.GrenadeAbility.AbilityActivated -= OnGrenadeAbility;
            Entity.MachineGunAbility.AbilityActivated -= OnMachineGunAbilityBulletShot;
            Entity.MissileAbility.AbilityActivated -= OnMissileAbility;
            Entity.DashAbility.AbilityActivated -= OnDashAbility;
        }
    }
}