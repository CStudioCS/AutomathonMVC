using UnityEngine;

namespace Automathon.Game
{
    public class WallView : EntityView<Wall>
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private NeonOutline outline;
        public override void Initialize(Wall wall)
        {
            base.Initialize(wall);

            spriteRenderer.size = new Vector2(wall.BoxCollider.Width / (float)WorldConstants.SPACE_SCALE, wall.BoxCollider.Height / (float)WorldConstants.SPACE_SCALE);

            // The NeonOutline is authored on the prefab; rebuild it now that the wall's size is known.
            if (outline != null) outline.Rebuild();
        }
    }

}