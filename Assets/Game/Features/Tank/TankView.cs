using UnityEngine;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private Transform turret;

        protected override void LateUpdate()
        {
            base.LateUpdate();

            Debug.Log(Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X)));
            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }
    }

}