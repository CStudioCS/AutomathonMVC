using Automathon.Engine.Physics;
using Automathon.Game.Utility;
using Automathon.Game.World;
using UnityEngine;


namespace Automathon.Game.BulletSystem
{
    public class BulletView : MonoBehaviour
    {
        private Bullet bullet;

        public void Initialize(Bullet bullet)
        {
            this.bullet = bullet;
            transform.localScale = Vector3.one * ((CircleCollider)bullet.Rigidbody.Collider).Radius / (float)WorldConstants.SPACE_SCALE;
        }

        private void LateUpdate()
        {
            transform.position = bullet.Position.ToVector2Scaled();
        }
    }
}