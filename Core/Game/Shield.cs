using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game
{
    public class Shield : Entity
    {
        public class ShieldState : Wall.WallState
        {
            public required Vector2Int Velocity;
        }

        public Rigidbody Rigidbody { get; private set; }
        private const int MAX_HEALTH = 1500;
        private const int LENGTH = 1500;
        private const int HEIGHT = 200;
        private const int LIFESPAN_MILLIS = 10000;

        public int Health { get; private set; } = MAX_HEALTH;
        public BoxCollider BoxCollider { get; private set; }

        public Shield(Vector2Int position, int rotationMillirad) : base(position)
        {
            RotationMilli = rotationMillirad;
            BoxCollider = new BoxCollider(Vector2Int.Zero, LENGTH, HEIGHT, 0);
            BoxCollider.Layer = CollisionLayer.Shield;
            Rigidbody = new Rigidbody(BoxCollider, 1, 1, 200);

            Initialize(Rigidbody, BoxCollider, new Health(MAX_HEALTH, true));

            AddBehavior(new Timer(LIFESPAN_MILLIS, null, () => GameplayManager.Destroy(this)));
        }

        public override State GetState()
            => new ShieldState() { Position = this.Position, RotationMilli = this.RotationMilli, Size = new Vector2Int(LENGTH, HEIGHT), Velocity = Rigidbody.Velocity };
    }
}
