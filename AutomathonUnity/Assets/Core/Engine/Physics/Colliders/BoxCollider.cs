using System;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public FixedVector2 WorldPosition => ParentEntity.Position + LocalCenterPosition;
        public FixedVector2 LocalCenterPosition { get; private set; }
        public Fixed Width { get; private set; }
        public Fixed Height { get; private set; }
        /// <summary>Rotation relative to the parent entity, in radians.</summary>
        public Fixed LocalRotation { get; private set; }
        /// <summary>World rotation in radians.</summary>
        public Fixed Rotation => ParentEntity.Rotation + LocalRotation;

        /// <summary>Corners in world space. Indexes: UL = 0, UR = 1, LR = 2, LL = 3 (in the box's local frame).</summary>
        public FixedVector2[] WorldVertices { get; private set; }

        public BoxCollider(FixedVector2 localPosition, Fixed width, Fixed height, Fixed localRotation) : base()
        {
            LocalCenterPosition = localPosition;
            Width = width;
            Height = height;
            LocalRotation = localRotation;
        }

        public override void Initialize(Entity parentEntity)
        {
            base.Initialize(parentEntity);
            PhysicsUpdate();
        }

        public override bool Colliding(Collider collider)
        {
            if (collider is BoxCollider b)
                return Collision.BoxBoxSAT(b, this).IsCollision;
            else if (collider is CircleCollider c)
                return Collision.BoxCircle(this, c);
            else if (collider is CompositeCollider composite)
                return composite.Colliding(this);
            else
                throw new NotImplementedException();
        }

        public override bool Contains(FixedVector2 point)
        {
            FixedVector2 r = point - WorldVertices[0];
            FixedVector2 right = WorldVertices[1] - WorldVertices[0];
            FixedVector2 down = WorldVertices[3] - WorldVertices[0];
            FixedLong sc1 = r.DotLong(right);
            FixedLong sc2 = r.DotLong(down);
            return sc1 >= FixedLong.Zero && sc1 <= right.LengthSquaredLong()
                && sc2 >= FixedLong.Zero && sc2 <= down.LengthSquaredLong();
        }

        public override void PhysicsUpdate()
        {
            FixedVector2 right = FixedVector2.FromAngle(Rotation);
            FixedVector2 up = right.OrthogonalCounterClockwise();
            FixedVector2 halfRight = right * (Width / 2);
            FixedVector2 halfUp = up * (Height / 2);
            FixedVector2 center = WorldPosition;

            WorldVertices = new FixedVector2[4]
            {
                center - halfRight + halfUp,
                center + halfRight + halfUp,
                center + halfRight - halfUp,
                center - halfRight - halfUp,
            };
        }
    }
}
