using Automathon.Game.World;
using UnityEngine;

namespace Automathon.Game.WallSystem
{
    public class WallView : MonoBehaviour
    {
        private Wall wall;
        SpriteRenderer rend;

        public void Initialize(Wall wall)
        {
            this.wall = wall;
            transform.localScale = new Vector3(wall.collider.Width / WorldConstants.SPACE_SCALE, wall.collider.Height / WorldConstants.SPACE_SCALE, 1);
        }
    }
}

