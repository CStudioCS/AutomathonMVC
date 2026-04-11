using System;

namespace Automathon.Engine.Physics
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> Added;
        public static event Action<Rigidbody> Removed;

        public int Friction; //should this be scaled ? probably, but tiz a problem for the future
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
