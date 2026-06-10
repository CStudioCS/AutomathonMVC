using Assets.Game.View;
using System.Collections;
using UnityEngine;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private float bulletShakingIntensity;
        [SerializeField] private float bulletShakingDuration;

        [SerializeField] private float grenadeShakingIntensity;
        [SerializeField] private float grenadeShakingDuration;

        [SerializeField] private float machineGunCameraShakingIntensity;
        private bool subbed;

        private CameraShaker cameraShaker;

        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);
            Entity.BulletAbility.AbilityActivated += OnShooting;
            Entity.MachineGunAbility.AbilityActivated += OnMachineGunAbility;
            Entity.GrenadeAbility.AbilityActivated += OnGrenadeAbility;
        }

        private void GrenadeAbility_AbilityActivated()
        {
            throw new System.NotImplementedException();
        }

        [SerializeField] private Transform turret;

        protected override void LateUpdate()
        {
            base.LateUpdate();

            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }

        private void OnShooting() //OnShoot is a unity message so don't rename this
        {
            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
        }

        private void OnMachineGunAbility()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }
            StartCoroutine(ShakeMultipleMachineGun());
        }

        private void OnShieldAbility()
        {
            // why not a shake
        }

        private void OnGrenadeAbility()
        {
            StartCoroutine(Shaker.Shake(turret, grenadeShakingDuration, grenadeShakingIntensity));
        }

        protected override void OnDestroy()
        {
            Entity.BulletAbility.AbilityActivated -= OnShooting;

            base.OnDestroy();
        }

        private IEnumerator ShakeMultipleMachineGun()
        {
            float intervalShootTime = Entity.MachineGunAbility.TimeToFireAllMilli / (Entity.MachineGunAbility.NumFiredBullets * 1000);
            for (int i = 0; i < Entity.MachineGunAbility.NumFiredBullets + 2; i++)// +2 just because it feels better
            {
                StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
                cameraShaker.CameraShake(intervalShootTime, machineGunCameraShakingIntensity);
                yield return new WaitForSeconds(intervalShootTime);
            }
        }
    }
}