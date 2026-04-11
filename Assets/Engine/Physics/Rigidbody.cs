using Automathon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> Added;
        public static event Action<Rigidbody> Removed;

        public Vector2Int Velocity;
        //this will have a bunch of stuff in the future stay tuned

        public Rigidbody() : base()
        {
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
    }
}
