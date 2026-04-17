using System;

namespace Automathon.Engine.Physics
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> Added;
        public static event Action<Rigidbody> Removed;

        public Collider Collider;

        public bool Kinematic;

        public Vector2Int Velocity;
        public int AngularVelocityMilli;
        public int Friction;

        public int InvMassMilli;
        public int InvI;

        //this will have a bunch of stuff in the future stay tuned, WIP !!!

        public Rigidbody(Collider collider)
        {
            Collider = collider;
            Added?.Invoke(this);
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Removed?.Invoke(this);
        }
    }
}
