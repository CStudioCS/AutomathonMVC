using Assets.Game.View;
using UnityEngine;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        private bool subbed;
        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);
            Entity.BulletAbility.AbilityActivated += OnShootBlabla;
        }

        [SerializeField] private Transform turret;

        protected override void LateUpdate()
        {
            base.LateUpdate();

            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }

        private void OnShootBlabla()
        {
            Debug.Log("here !");
            StartCoroutine(Shaker.Shake(turret, 0.1f, 0.05f));
        }

        protected override void OnDestroy()
        {
            Entity.BulletAbility.AbilityActivated -= OnShootBlabla;

            base.OnDestroy();
        }
    }

}