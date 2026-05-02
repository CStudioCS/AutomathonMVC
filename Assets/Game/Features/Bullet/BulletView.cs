using Automathon.Game.Utility;
using Automathon.Game.View;
using Automathon.Game.World;
using UnityEngine;


namespace Automathon.Game.BulletSystem
{
    public class BulletView : EntityView<Bullet>
    {
        private Bullet bullet;

        public override void Initialize(Bullet bullet)
        {
            base.Initialize(bullet);
            transform.localScale = Vector3.one * 2 * bullet.CircleCollider.Radius / (float)WorldConstants.SPACE_SCALE;
        }

        private void LateUpdate()
        {
            transform.position = bullet.Position.ToVector2Scaled();
        }
    }
}