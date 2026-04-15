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
            transform.localScale = new Vector3(wall.collider.Width, wall.collider.Height, 1);
        }



    }

}

