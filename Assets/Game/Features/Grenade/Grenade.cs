

using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Engine.Utility;
using Automathon.Game.BulletSystem;
using Automathon.Utility;

namespace Automathon.Game.GrenadeSystem
{
    public class Grenade : Entity
    {
        private const int RADIUS = 300;
        private const int SPEED = 1800;
        private const int EXPLOSION_DELAY = 2000;
        private const int FRAGMENT_NUMBER = 12;

        public CircleCollider CircleCollider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public Grenade(Vector2Int position, Vector2Int direction) : base(position)
        {
            CircleCollider = new CircleCollider(Vector2Int.Zero, RADIUS);
            Rigidbody = new Rigidbody(CircleCollider, 1000, 500, 200);

            Initialize(CircleCollider, Rigidbody);

            Rigidbody.Velocity = direction * SPEED / 1000;

            AddBehavior(new Timer(EXPLOSION_DELAY, null, BlowUp));
        }

        private void BlowUp()
        {
            for (int i = 0; i < FRAGMENT_NUMBER; i++)
            {
                int theta = i * IntMath.PI_MILLI * 2 / FRAGMENT_NUMBER;

                Vector2Int dir = new Vector2Int(TrigTable.Cos(theta), TrigTable.Sin(theta));

                //calcule la distance min du centre pour que les fragments ne se touchent pas
                int alpha = IntMath.PI_MILLI * 2 / FRAGMENT_NUMBER;

                // * 1200 / 1000 in order to space them out enough, it's kinda arbitrary but it works
                int dist = 2 * Bullet.RADIUS * TrigTable.Cos(alpha) / TrigTable.Sin(alpha) * 1200 / 1000;

                Bullet bullet = new Bullet(Position + dir * dist / 1000, dir);
                GameplayManager.Instantiate(bullet);
            }

            GameplayManager.Destroy(this);
        }

    }
}