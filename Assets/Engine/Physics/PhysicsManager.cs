using System;
using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public class PhysicsManager : IDisposable
    {
        public readonly List<Rigidbody> rigidbodies = new();
        public readonly List<Collider> colliders = new();

        public PhysicsManager()
        {
            Rigidbody.Added += OnRigidbodyAdded;
            Rigidbody.Removed += OnRigidbodyRemoved;
            Collider.Added += OnColliderAdded;
            Collider.Removed += OnColliderRemoved;
        }

        public void Dispose()
        {
            Rigidbody.Added -= OnRigidbodyAdded;
            Rigidbody.Removed -= OnRigidbodyRemoved;
            Collider.Added -= OnColliderAdded;
            Collider.Removed -= OnColliderRemoved;
        }

        private void OnRigidbodyAdded(Rigidbody rigidbody)
            => rigidbodies.Add(rigidbody);

        private void OnRigidbodyRemoved(Rigidbody rigidbody)
            => rigidbodies.Remove(rigidbody);

        private void OnColliderAdded(Collider collider)
            => colliders.Add(collider);

        private void OnColliderRemoved(Collider collider)
            => colliders.Remove(collider);

        public void Step()
        {
            foreach(Collider collider in colliders)
                collider.PhysicsUpdate();

            //This is temporary and sucks balls
            foreach(Rigidbody rb in rigidbodies)
            {
                bool blocked = false;
                if(!rb.ParentEntity.TryGetComponent(out Collider c))
                    continue;

                foreach (Collider collider in colliders)
                {
                    if (collider != c && c.CollideAt(rb.ParentEntity.Position + rb.Velocity / GameplayConstants.Framerate, collider))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (!blocked)
                    rb.ParentEntity.Position += rb.Velocity / GameplayConstants.Framerate; //I wanna use deltatime :(
            }
        }
    }
}
