using Automathon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> OnAdded;
        public static event Action<Rigidbody> OnRemoved;

        public Vector2Int Velocity;

        public Rigidbody(Entity parentEntity) : base(parentEntity)
        {
        }
    }
}
