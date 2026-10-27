using Automathon.Engine;
using Automathon.Engine.Physics;
using System;

namespace Automathon.Game
{
    public class Grenade : Entity
    {
        public class GrenadeState : State
        {
            public Fixed Radius;
            public FixedVector2 Velocity;
            public int NumFramesLeftBeforeExplosion;
        }

        private int numFramesLeftBeforeExplosion;
        private static readonly Fixed RADIUS = 0.3;
        private static readonly Fixed SPEED = 1.8;           // units per second
        private static readonly Fixed EXPLOSION_DELAY = 2;   // seconds
        private const int FRAGMENT_NUMBER = 12;
        public Action Explode;

        public CircleCollider CircleCollider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public Grenade(FixedVector2 position, FixedVector2 direction) : base(position)
        {
            CircleCollider = new CircleCollider(FixedVector2.Zero, RADIUS);
            Rigidbody = new Rigidbody(CircleCollider, invMass: 1, invInertia: 0.5, friction: 0);

            Initialize(CircleCollider, Rigidbody);

            Rigidbody.Velocity = direction.Normalized() * SPEED;

            AddBehavior(new Timer(EXPLOSION_DELAY, (t) => numFramesLeftBeforeExplosion = t.ValueFrames, BlowUp));
        }

        private void BlowUp()
        {
            Explode?.Invoke();

            // angle between two fragments
            Fixed alpha = Fixed.TwoPi / FRAGMENT_NUMBER;

            // minimum distance from the center so that the fragments don't touch each other
            // (* 1.2 in order to space them out enough, it's kinda arbitrary but it works)
            Fixed dist = 2 * Bullet.RADIUS * FixedMath.Cos(alpha) / FixedMath.Sin(alpha) * 1.2;

            for (int i = 0; i < FRAGMENT_NUMBER; i++)
            {
                FixedVector2 dir = FixedVector2.FromAngle(alpha * i);

                Bullet bullet = new Bullet(Position + dir * dist, dir, null);
                GameplayManager.Instantiate(bullet);
            }

            GameplayManager.Destroy(this);
        }

        public override State GetState()
        {
            return new GrenadeState()
            {
                Position = Position,
                Radius = RADIUS,
                Velocity = Rigidbody.Velocity,
                NumFramesLeftBeforeExplosion = numFramesLeftBeforeExplosion,
            };
        }
    }
}
