using System;

namespace Automathon.Engine.Physics
{
    public abstract class Collider : Component
    {
        public Action<CollisionEvent> OnCollision;

        public abstract bool Colliding(Collider collider);
        public abstract bool Contains(Vector2Int point);
        public virtual void PhysicsUpdate() { }
    }
}
