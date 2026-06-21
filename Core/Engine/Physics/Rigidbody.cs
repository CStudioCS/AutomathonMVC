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

        public Rigidbody(Collider collider, int invMassMilli, int invIMicro, int frictionMilli)
        {
            Collider = collider;
            InvMassMilli = invMassMilli;
            InvIMicro = invIMicro;
            FrictionMilli = frictionMilli;

            Added?.Invoke(this);
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Removed?.Invoke(this);
        }
    }
}
