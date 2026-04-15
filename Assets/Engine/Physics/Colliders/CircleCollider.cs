using Automathon.Physics;
using System;

namespace Automathon.Engine.Physics
{
    public class CircleCollider : Collider
    {
        public Vector2Int WorldPos => ParentEntity.Position + LocalPosition;
        public Vector2Int LocalPosition { get; private set; }
        public int Radius { get; private set; }

        public CircleCollider(Vector2Int localPosition, int radius)
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
            else
                throw new NotImplementedException();
        }
    }
}
