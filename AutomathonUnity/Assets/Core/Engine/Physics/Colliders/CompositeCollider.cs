using Automathon.Engine.Physics;

namespace Automathon.Engine
{
    public class CompositeCollider : Collider
    {
        public Collider[] Colliders;
        public CompositeCollider(params Collider[] colliders)
        {
            Colliders = colliders;
        }

        public override void Initialize(Entity parentEntity)
        {
            base.Initialize(parentEntity);
            foreach (Collider collider in Colliders)
                collider.Initialize(parentEntity);
        }

        public override void PhysicsUpdate()
        {
            foreach (Collider collider in Colliders)
                collider.PhysicsUpdate();
        }

        public override bool Colliding(Collider collider)
        {
            foreach (Collider c in Colliders)
                if (c.Colliding(collider))
                    return true;
            return false;
        }

        public override bool Contains(FixedVector2 point)
        {
            foreach (Collider c in Colliders)
                if (c.Contains(point))
                    return true;

            return false;
        }
    }
}
