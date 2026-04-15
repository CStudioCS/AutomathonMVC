using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game.WallSystem
{
    public class Wall : Entity
    {
        public BoxCollider collider;
        public Rigidbody body;
        public Wall(Vector2Int position, int halfWidth, int halfHeight) : base(position)
        {
            collider = new BoxCollider(position, halfWidth, halfHeight, 0);
            body = new Rigidbody(collider);
            Initialize(collider, body);
        }
    }
}
