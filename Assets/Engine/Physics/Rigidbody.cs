using System;

namespace Automathon.Engine.Physics
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> Added;
        public static event Action<Rigidbody> Removed;

        public Collider Collider;

        public Vector2Int Velocity;
        public int AngularVelocityMilli;
        public int FrictionMilli;

        public int InvMassMilli;
        public int InvIMicro;

        public Vector2Int Forces;
        public int TorqueMilli;

        //this will have a bunch of stuff in the future stay tuned, WIP !!!

        public Rigidbody(Collider collider, int invMassMilli, int invIMicro, int frictionMilli)
        {
            Collider = collider;
            Added?.Invoke(this);

            InvMassMilli = invMassMilli;
            InvIMicro = invIMicro;
            FrictionMilli = frictionMilli;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Removed?.Invoke(this);
        }
    }
}
