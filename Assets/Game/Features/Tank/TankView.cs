using Assets.Game.View;
using System.Collections;
using UnityEngine;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private float bulletShakingIntensity;
        [SerializeField] private float bulletShakingDuration;

        [SerializeField] private float bulletCameraShakingIntensity;
        [SerializeField] private float bulletCameraShakingDuration;
        private bool subbed;

        [SerializeField] private float DashShakingIntensity;
        [SerializeField] private float DashCameraShakingIntensity;

        private CameraShaker cameraShaker;

        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);
            Entity.BulletAbility.AbilityActivated += OnShooting;
            Entity.MachineGunAbility.BulletShot += OnMachineGunAbilityBulletShot;
            Entity.GrenadeAbility.AbilityActivated += OnGrenadeAbility;
            Entity.DashAbility.AbilityActivated += OnDashAbility;

            tank = entity;
        }

        [SerializeField] private Transform turret;
        [SerializeField] private Transform body;
        [SerializeField] private ParticleSystem dashBurstParticleSystem;
        [SerializeField] private ParticleSystem dashFlame;

        private Tank tank;

        protected override void LateUpdate()
        {
            base.LateUpdate();

            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }

        private void OnShooting() //OnShoot is a unity message so don't rename this
        {
            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
        }

        private void OnMachineGunAbilityBulletShot()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }

            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
            cameraShaker.CameraShake(bulletShakingDuration, bulletCameraShakingIntensity);
        }

        IEnumerator Dash(int dashDurationMili)
        {
            float dashDuration = (float)dashDurationMili / 1000f;
            dashFlame.Play();
            print("poop");
            float timer = -0.10f;

            while (timer < dashDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            dashFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDashAbility()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }
            StartCoroutine(Shaker.Translate(turret, new Vector2(0, -1), DashAbility.DASH_DURATION_MILLIS, DashShakingIntensity));
            cameraShaker.CameraTranslate(body.right,DashAbility.DASH_DURATION_MILLIS, DashCameraShakingIntensity);
            //dashBurstParticleSystem.Play();  imo pas besoin de burst initiale mais bon c'est implémenté quoi
            StartCoroutine(Dash(DashAbility.DASH_DURATION_MILLIS));
        }

        private void OnGrenadeAbility()
        {
            //No shake when shooting a grenade imo
            //StartCoroutine(Shaker.Shake(turret, grenadeShakingDuration, grenadeShakingIntensity));
        }

        protected override void OnDestroy()
        {
            Entity.BulletAbility.AbilityActivated -= OnShooting;
            Entity.MachineGunAbility.BulletShot -= OnMachineGunAbilityBulletShot;
            Entity.GrenadeAbility.AbilityActivated -= OnGrenadeAbility;

            base.OnDestroy();
        }

        protected override void OnControllerDestroyed()
        {
            Entity.BulletAbility.AbilityActivated -= OnShooting;
            Entity.MachineGunAbility.AbilityActivated -= OnMachineGunAbilityBulletShot;
            Entity.GrenadeAbility.AbilityActivated -= OnGrenadeAbility;
            Entity.DashAbility.AbilityActivated -= OnDashAbility;
            base.OnControllerDestroyed();
        }
    }
}