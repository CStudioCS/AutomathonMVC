using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game
{
    public class InvisibleWall : Entity
    {
        public class WallState : State
        {
            public FixedVector2 Size;
            /// <summary>Rotation in radians.</summary>
            public Fixed Rotation;
        }

        public FixedVector2 Size;
        public Rigidbody Rigidbody { get; private set; }
        public BoxCollider BoxCollider { get; private set; }

        public InvisibleWall(FixedVector2 position, FixedVector2 size, Fixed rotation) : base(position)
        {
            Rotation = rotation;
            Size = size;
            BoxCollider = new BoxCollider(FixedVector2.Zero, size.X, size.Y, 0);
            this.BoxCollider.Layer = CollisionLayer.InvisibleWall;
            Rigidbody = new Rigidbody(BoxCollider, invMass: 0, invInertia: 0, friction: 0);

            Initialize(BoxCollider, Rigidbody);
        }

        public override State GetState()
            => new WallState() { Position = this.Position, Rotation = Rotation, Size = this.Size };
    }
}
