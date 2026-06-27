using UnityEngine;

namespace Automathon.Game
{
    public class WallView : EntityView<Wall>
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        public override void Initialize(Wall wall)
        {
            base.Initialize(wall);

            spriteRenderer.size = new Vector2(wall.BoxCollider.Width / (float)WorldConstants.SPACE_SCALE, wall.BoxCollider.Height / (float)WorldConstants.SPACE_SCALE);
        }
    }

}