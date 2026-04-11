using System;

namespace Automathon.Engine.Physics
{
    public abstract class Collider : Component
    {
        public bool isTrigger;
        public event Action<Collider> OnTriggerEnter;
        public event Action<Collider> OnTriggerStay;
        public event Action<Collider> OnTriggerExit;

        public event Action<Collider> OnCollision;

        public abstract bool Colliding(Collider collider);

        public virtual void PhysicsUpdate() { }
    }
}
