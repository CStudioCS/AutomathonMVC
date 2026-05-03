

using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.BulletSystem;
using Automathon.Utility;

namespace Automathon.Game.GrenadeSystem
{
    public class Grenade : Entity
    {
        private const int BULLET_SPEED = 1500;
        private const int FRAGMENT_RADIUS = 1000 / 50;

        public CircleCollider CircleCollider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public Grenade(Vector2Int position, Vector2Int direction, int speed, int delayMilisecond, int fragmentNumber) : base(position)
        {
            CircleCollider = new CircleCollider(Vector2Int.Zero, 1000 / 2);
            Rigidbody = new Rigidbody(CircleCollider, 1000, 500, 200);
            Rigidbody.Velocity = direction * speed / 1000;

            Initialize(CircleCollider, Rigidbody);

            AddBehavior(new Timer(delayMilisecond, null, () => BlowUp(fragmentNumber)));
        }

        private void BlowUp(int numBullets)
        {
            for (int i = 0; i < numBullets; i++)
            {
                int theta = i * 6283 / numBullets;

                Vector2Int dir = new Vector2Int(TrigTable.Cos(theta), TrigTable.Sin(theta));

                //calcule la distance min du centre pour que les fragments ne se touchent pas
                int alpha = 6283 / numBullets;
                int dist = 2 * FRAGMENT_RADIUS * TrigTable.Cos(alpha) / TrigTable.Sin(alpha) * 1100 / 1000;

                Bullet bullet = new Bullet(Position + dir * dist / 1000, dir, BULLET_SPEED, FRAGMENT_RADIUS);
                GameplayManager.Instantiate(bullet);
            }

            GameplayManager.Destroy(this);
        }

    }
}