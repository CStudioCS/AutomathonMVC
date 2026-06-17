using Assets.Game.View;
using UnityEngine;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private float bulletShakingIntensity;
        [SerializeField] private float bulletShakingDuration;

        [SerializeField] private float bulletCameraShakingIntensity;
        [SerializeField] private float bulletCameraShakingDuration;

        private CameraShaker cameraShaker;

        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);
            //Entity.BulletAbility.AbilityActivated += OnShooting;
            Entity.MachineGunAbility.BulletShot += OnMachineGunAbilityBulletShot;
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

        private void OnMachineGunAbilityBulletShot()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }

            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
            cameraShaker.CameraShake(bulletShakingDuration, bulletCameraShakingIntensity);
        }


        protected override void OnDestroy()
        {
            //Entity.BulletAbility.AbilityActivated -= OnShooting;
            Entity.MachineGunAbility.BulletShot -= OnMachineGunAbilityBulletShot;

            base.OnDestroy();
        }

        protected override void OnControllerDestroyed()
        {
            //Entity.BulletAbility.AbilityActivated -= OnShooting;
            Entity.MachineGunAbility.AbilityActivated -= OnMachineGunAbilityBulletShot;
            base.OnControllerDestroyed();
        }
    }
}