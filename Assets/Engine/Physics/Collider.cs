using System;

namespace Automathon.Engine.Physics
{
    public abstract class Collider : Component
    {
        public bool isTrigger;
        public Action<Collider> OnTriggerEnter;
        public Action<Collider> OnTriggerStay;
        public Action<Collider> OnTriggerExit;

        public Action<Collider> OnCollision;

        public abstract bool Colliding(Collider collider);
        public abstract bool Contains(Vector2Int point);
        public virtual void PhysicsUpdate() { }
    }
}
