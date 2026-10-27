using System;

namespace Automathon.Engine.Physics
{
    public abstract class Collider : Component
    {
        public bool IsTrigger;
        public CollisionLayer Layer = CollisionLayer.Default;
        public Action<CollisionEvent> OnCollision;

        public abstract bool Colliding(Collider collider);
        public abstract bool Contains(FixedVector2 point);
        public virtual void PhysicsUpdate() { }
    }
}
