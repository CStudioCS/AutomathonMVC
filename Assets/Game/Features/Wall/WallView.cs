using Automathon.Game.Utility;
using Automathon.Game.World;
using Automathon.Utility;
using UnityEngine;

namespace Automathon.Game.WallSystem
{
    public class WallView : MonoBehaviour
    {
        private Wall wall;

        public void Initialize(Wall wall)
        {
            this.wall = wall;

            transform.position = wall.Position.ToVector2Scaled();
            transform.localScale = new Vector3(wall.BoxCollider.Width / (float)WorldConstants.SPACE_SCALE, wall.BoxCollider.Height / (float)WorldConstants.SPACE_SCALE, 1);
            transform.rotation = ViewMath.MilliRadRotationToQuaternion(wall.BoxCollider.RotationMillirad);
        }
    }

}