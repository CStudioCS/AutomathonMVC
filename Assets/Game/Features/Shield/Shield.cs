using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.HealthSystem;

namespace Automathon.Game.ShieldSystem
{
    public class Shield : Entity
    {
        public Rigidbody Rigidbody { get; private set; }
        private const int MAX_HEALTH = 400;
        private const int HALF_LENGTH = 750;
        private const int HALF_HEIGHT = 100;
        private const int LIFESPAN_MILLIS = 10000;

        public int Health { get; private set; } = MAX_HEALTH;
        public BoxCollider BoxCollider { get; private set; }
        public Shield(Vector2Int position, int rotationMillirad) : base(position)
        {
            RotationMilli = rotationMillirad;
            BoxCollider = new BoxCollider(Vector2Int.Zero, HALF_LENGTH, HALF_HEIGHT, 0);
            Rigidbody = new Rigidbody(BoxCollider, 1000, 500, 200);

            Initialize(
                Rigidbody,
                BoxCollider,
                new Health(MAX_HEALTH, true)
                );

            AddBehavior(new Timer(LIFESPAN_MILLIS, null, () => GameplayManager.Destroy(this)));
        }
    }
}
