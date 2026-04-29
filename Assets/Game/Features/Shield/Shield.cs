using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game.ShieldSystem
{
    public class Shield : Entity
    {
        public Rigidbody Rigidbody { get; private set; }
        private const int shieldHalfLength = 750;
        private const int shieldHalfHeight = 100;
        public BoxCollider boxCollider { get; private set; }
        public Shield(Vector2Int position, int rotationMillirad) : base(position)
        {
            boxCollider = new BoxCollider(Vector2Int.Zero, shieldHalfLength, shieldHalfHeight, rotationMillirad);
            Rigidbody = new Rigidbody(boxCollider);
            Initialize(Rigidbody, boxCollider);
        }
    }
}
