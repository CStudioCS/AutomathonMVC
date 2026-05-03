using Automathon.Game.View;
using Automathon.Game.World;
using UnityEngine;

namespace Automathon.Game.WallSystem
{
    public class WallView : EntityView<Wall>
    {
        public override void Initialize(Wall wall)
        {
            base.Initialize(wall);

            transform.localScale = new Vector3(wall.BoxCollider.Width / (float)WorldConstants.SPACE_SCALE, wall.BoxCollider.Height / (float)WorldConstants.SPACE_SCALE, 1);
        }
    }

}