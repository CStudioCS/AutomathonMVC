using System;
using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public class PhysicsManager : IDisposable
    {
        public readonly List<Rigidbody> rigidbodies = new();

        public PhysicsManager()
        {
            Rigidbody.Added += OnRigidbodyAdded;
            Rigidbody.Removed += OnRigidbodyRemoved;
        }

        public void Dispose()
        {
            Rigidbody.Added -= OnRigidbodyAdded;
            Rigidbody.Removed -= OnRigidbodyRemoved;
        }

        private void OnRigidbodyAdded(Rigidbody rigidbody)
            => rigidbodies.Add(rigidbody);

        private void OnRigidbodyRemoved(Rigidbody rigidbody)
            => rigidbodies.Remove(rigidbody);

        public void Step()
        {
            foreach (Rigidbody rigidbody in rigidbodies)
                rigidbody.Collider.PhysicsUpdate();

            //This is temporary and sucks balls
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                bool blocked = false;

                foreach (Rigidbody otherRigidbody in rigidbodies)
                {
                    if (otherRigidbody == rigidbody)
                        continue;

                    if (rigidbody.Collider.CollideAt(otherRigidbody.Collider, rigidbody.ParentEntity.Position + rigidbody.Velocity / GameplayConstants.FRAMERATE))
                    {
                        blocked = true;
                        rigidbody.Collider.OnCollision?.Invoke(otherRigidbody.Collider);
                        break;
                    }
                }

                if (!blocked)
                    rigidbody.ParentEntity.Position += rigidbody.Velocity / GameplayConstants.FRAMERATE; //I wanna use deltatime :(
            }
        }
    }
}
