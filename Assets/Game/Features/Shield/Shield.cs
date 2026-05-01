using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game.ShieldSystem
{
    public class Shield : Entity
    {
        public Rigidbody Rigidbody { get; private set; }
        private const int SHIELD_HALF_LENGTH = 750;
        private const int SHIELD_HALF_HEIGHT = 100;
        public BoxCollider BoxCollider { get; private set; }
        public Shield(Vector2Int position, int rotationMillirad) : base(position)
        {
            BoxCollider = new BoxCollider(Vector2Int.Zero, SHIELD_HALF_LENGTH, SHIELD_HALF_HEIGHT, rotationMillirad);
            Rigidbody = new Rigidbody(BoxCollider);
            Initialize(Rigidbody, BoxCollider);
        }
    }
}
