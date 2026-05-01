

using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.BulletSystem;
using Automathon.Game.World;
using Automathon.Utility;
using System;

namespace Automathon.Game.GrenadeSystem
{
    public class Grenade : Entity
    {
        public static event Action<Grenade> OnSpawned;
        public event Action OnBlowedUp;

        public GameplayManager gameplayManager;

        public CircleCollider CircleCollider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        private const int bulletSpeed = 1500;
        private const int fragmentRadius = 1000 / 50;

        public Grenade(Vector2Int position, Vector2Int direction, int speed, int delayMilisecond, int fragmentNumber) : base(position)
        {
            CircleCollider = new CircleCollider(position, 1000/2);
            Rigidbody = new Rigidbody(CircleCollider);
            Rigidbody.Velocity = direction * speed / 1000;

            Initialize(CircleCollider, Rigidbody);

            AddBehavior(new Timer(500, null, () => BlowUp(fragmentNumber)));

            OnSpawned?.Invoke(this);
        }

        private void BlowUp(int numBullets)
        {
            for(int i = 0; i < numBullets; i ++)
            {
                int theta = i * 6283 / numBullets;

                Vector2Int dir = new Vector2Int(TrigTable.Cos(theta), TrigTable.Sin(theta));

                //calcule la distance min du centre pour que les fragments ne se touchent pas
                int alpha = 6283 / numBullets;
                int dist = 2 * fragmentRadius * TrigTable.Cos(alpha) / TrigTable.Sin(alpha) * 1100 / 1000;

                Bullet bullet = new Bullet(this.Position + dir * dist / 1000, dir, bulletSpeed, fragmentRadius);
                gameplayManager.Instantiate(bullet);
            }
            OnBlowedUp?.Invoke();
        }

    }
}