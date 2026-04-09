using System;
using System.Collections.Generic;

namespace Automathon.Engine
{
    public class PhysicsManager : IDisposable
    {
        public readonly List<Rigidbody> rigidbodies = new();

        public PhysicsManager()
        {
            Rigidbody.OnAdded += OnRigidbodyAdded;
            Rigidbody.OnRemoved += OnRigidbodyRemoved;
        }

        public void Dispose()
        {
            Rigidbody.OnAdded -= OnRigidbodyAdded;
            Rigidbody.OnRemoved -= OnRigidbodyRemoved;
        }

        private void OnRigidbodyAdded(Rigidbody rigidbody)
            => rigidbodies.Add(rigidbody);

        private void OnRigidbodyRemoved(Rigidbody rigidbody)
            => rigidbodies.Remove(rigidbody);
    }
}
