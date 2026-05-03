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
            RotationMilli = rotationMillirad;
            BoxCollider = new BoxCollider(Vector2Int.Zero, SHIELD_HALF_LENGTH, SHIELD_HALF_HEIGHT, 0);
            Rigidbody = new Rigidbody(BoxCollider, 1000, 500, 200);
            Initialize(Rigidbody, BoxCollider);
        }
    }
}
