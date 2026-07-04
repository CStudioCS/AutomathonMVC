using Automathon.Engine.Physics;

namespace Automathon.Engine
{
    public class CompositeCollider : Collider
    {
        public Collider[] Colliders;
        public CompositeCollider(params Collider[] colliders)
        {
            Colliders = colliders;
            foreach (Collider collider in Colliders)
                collider.Initialize(ParentEntity);
        }

        public override bool Colliding(Collider collider)
        {
            foreach (Collider c in Colliders)
                if (c.Colliding(collider))
                    return true;
            return false;
        }

        public override bool Contains(Vector2Int point)
        {
            foreach (Collider c in Colliders)
                if (c.Contains(point))
                    return true;

            return false;
        }
    }
}
