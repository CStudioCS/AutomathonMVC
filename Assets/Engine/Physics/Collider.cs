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

        public static event Action<Collider> Added;
        public static event Action<Collider> Removed;

        public abstract bool Colliding(Collider collider);

        public bool CollideAt(Vector2Int pos, Collider other)
        {
            Vector2Int oldPos = ParentEntity.Position;
            ParentEntity.Position = pos;
            bool colliding = Colliding(other);
            ParentEntity.Position = oldPos;
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
