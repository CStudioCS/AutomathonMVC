using Automathon.Game.BulletSystem;
using Automathon.Game.TankSystem;
using Automathon.Game.World;
using System;
using UnityEngine;

namespace Automathon.Game.WallSystem
{
    public class WallView : MonoBehaviour
    {
        private Wall wall;

        public void Initialize(Wall wall)
        {
            this.wall = wall;

            transform.localScale = new Vector3(wall.BoxCollider.Width / 2.0f / (float)WorldConstants.SPACE_SCALE, wall.BoxCollider.Height / 2.0f / (float)WorldConstants.SPACE_SCALE, 1);
            transform.rotation = Quaternion.Euler(0, 0, ((Automathon.Engine.Physics.BoxCollider)wall.Rigidbody.Collider).RotationMillirad / 1000f * Mathf.Rad2Deg);
        }
    }

}