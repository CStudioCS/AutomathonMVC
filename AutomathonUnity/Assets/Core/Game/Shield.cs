using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game
{
    public class Shield : Entity
    {
        public class ShieldState : Wall.WallState
        {
            public Fixed Health;
            public FixedVector2 Velocity;
        }

        public Rigidbody Rigidbody { get; private set; }
        private Health health;
        private static readonly Fixed MAX_HEALTH = 1500;
        private static readonly Fixed LENGTH = 1.5;
        private static readonly Fixed HEIGHT = 0.2;
        private static readonly Fixed LIFESPAN = 10; // seconds

        public Fixed Health => health.CurrentHealth;
        public BoxCollider BoxCollider { get; private set; }

        public Shield(FixedVector2 position, Fixed rotation) : base(position)
        {
            Rotation = rotation;
            BoxCollider = new BoxCollider(FixedVector2.Zero, LENGTH, HEIGHT, 0);
            BoxCollider.Layer = CollisionLayer.Shield;
            // Very heavy: barely moves when hit
            Rigidbody = new Rigidbody(BoxCollider, invMass: 0.001, invInertia: 0.001, friction: 0);

            Initialize(Rigidbody, BoxCollider, health = new Health(MAX_HEALTH, true));

            AddBehavior(new Timer(LIFESPAN, null, () => GameplayManager.Destroy(this)));
        }

        public override State GetState()
            => new ShieldState() { Position = this.Position, Rotation = this.Rotation, Size = new FixedVector2(LENGTH, HEIGHT), Velocity = Rigidbody.Velocity, Health = health.CurrentHealth };
    }
}
