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

        public static event Action<Collider> Added;
        public static event Action<Collider> Removed;

        public abstract bool Colliding(Collider collider);

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

        public override void Start()
        {
            Added(this);
            base.Start();
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            Removed(this);
        }

        public virtual void PhysicsUpdate() { }
    }
}
