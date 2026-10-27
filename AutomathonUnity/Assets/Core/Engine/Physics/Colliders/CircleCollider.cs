using System;

namespace Automathon.Engine.Physics
{
    public class CircleCollider : Collider
    {
        public FixedVector2 WorldPosition => ParentEntity.Position + LocalPosition;
        public FixedVector2 LocalPosition { get; private set; }
        public Fixed Radius { get; private set; }

        public CircleCollider(FixedVector2 localPosition, Fixed radius) : base()
        {
            LocalPosition = localPosition;
            Radius = radius;
        }

        public override bool Colliding(Collider collider)
        {
            if (collider is BoxCollider b)
                return Collision.BoxCircle(b, this);
            else if (collider is CircleCollider c)
                return Collision.CircleCircle(this, c);
            else if (collider is CompositeCollider composite)
                return composite.Colliding(this);
            else
                throw new NotImplementedException();
        }

        public override bool Contains(FixedVector2 point)
            => (point - WorldPosition).LengthSquaredLong() <= FixedLong.Multiply(Radius, Radius);
    }
}
