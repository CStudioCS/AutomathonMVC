using Automathon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Features.Physics
{
    public class Rigidbody : Component
    {
        public Vector2Int Velocity;

        public Rigidbody(Entity parentEntity) : base(parentEntity)
        {
        }
    }
}
