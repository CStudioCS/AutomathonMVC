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

        //this sucks and will be removed with the new physics engine
        public bool CollideAt(Collider other, Vector2Int pos)
        {
            Vector2Int oldPos = ParentEntity.Position;
            ParentEntity.Position = pos;
            PhysicsUpdate();
            bool colliding = Colliding(other);
            ParentEntity.Position = oldPos;
            PhysicsUpdate();
            return colliding;
        }

        public virtual void PhysicsUpdate() { }
    }
}
