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
        public int FrictionMilli;

        public int InvMassMilli;
        public int InvIMilli;

        public Vector2Int Forces;
        public int TorqueMilli;

        //this will have a bunch of stuff in the future stay tuned, WIP !!!

        public Rigidbody(Collider collider)
        {
            Collider = collider;
            Added?.Invoke(this);

            //TEMPORARY
            InvMassMilli = 1000;
            if (collider is BoxCollider box)
                InvIMilli = 1000 / ((box.Width * box.Width + box.Height * box.Height) / 12);
            else if (collider is CircleCollider circle)
                InvIMilli = 1000 / (circle.Radius * circle.Radius / 2);
            
            FrictionMilli = 200;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Removed?.Invoke(this);
        }
    }
}
