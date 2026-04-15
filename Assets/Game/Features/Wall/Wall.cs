using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.World;

namespace Automathon.Game.WallSystem
{
    public class Wall : Entity
    {
        public BoxCollider collider;
        public Rigidbody body;
        public Wall(Vector2Int position, int width, int height) : base(position)
        {
            collider = new BoxCollider(position, width * WorldConstants.SPACE_SCALE / 2, height * WorldConstants.SPACE_SCALE / 2, 0);
            body = new Rigidbody(collider);
            Initialize(collider, body);
        }
    }
}
