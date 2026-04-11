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
            //Physics Update all colliders here

            //TODO: Step one physics frame ! (maybe at a different deltaTime than Update ?)
        }
    }
}
