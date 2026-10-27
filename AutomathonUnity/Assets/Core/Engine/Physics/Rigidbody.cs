using System;

namespace Automathon.Engine.Physics
{
    public class Rigidbody : Component
    {
        public static event Action<Rigidbody> Added;
        public static event Action<Rigidbody> Removed;

        public Collider Collider;

        /// <summary>Linear velocity in units per second.</summary>
        public FixedVector2 Velocity;
        /// <summary>Angular velocity in radians per second.</summary>
        public Fixed AngularVelocity;
        public Fixed Friction;

        /// <summary>1 / mass. 0 means infinite mass (static body).</summary>
        public Fixed InvMass;
        /// <summary>1 / moment of inertia. 0 means the body can't be rotated by collisions.</summary>
        public Fixed InvInertia;

        /// <summary>Forces accumulated during the frame, cleared after each physics step.</summary>
        public FixedVector2 Forces;
        public Fixed Torque;

        public Rigidbody(Collider collider, Fixed invMass, Fixed invInertia, Fixed friction)
        {
            Collider = collider;
            InvMass = invMass;
            InvInertia = invInertia;
            Friction = friction;

            Added?.Invoke(this);
        }

        public void AddForce(FixedVector2 force) => Forces += force;
        public void AddTorque(Fixed torque) => Torque += torque;

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Removed?.Invoke(this);
        }
    }
}
