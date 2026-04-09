using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine
{
    public abstract class Collider
    {
        public bool isTrigger;
        public event Action<Collider> OnTriggerEnter;
        public event Action<Collider> OnTriggerStay;
        public event Action<Collider> OnTriggerExit;

        public event Action<Collider> OnCollision;

        public abstract bool Colliding(Collider collider);
    }
}
