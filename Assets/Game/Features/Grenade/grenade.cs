

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
        public static event Action<Grenade> Spawned;
        public event Action Blowed;

        public GameplayManager gameplayManager;

        public CircleCollider CircleCollider { get; private set; }
        public Rigidbody Rb { get; private set; }

        public Grenade(Vector2Int position, Vector2Int direction, int speed, int delayMilisecond, int fragNumber, int fragSpeed) : base(position)
        {
            CircleCollider = new CircleCollider(position, WorldConstants.SPACE_SCALE/2);
            Rb = new Rigidbody(CircleCollider);
            Rb.Velocity = direction * speed / WorldConstants.SPACE_SCALE;

            Initialize(CircleCollider, Rb);

            this.AddBehavior(new Timer(500, null, () => BlowUp(fragNumber, fragSpeed)));

            Spawned?.Invoke(this);
        }

        private void BlowUp(int n, int speed)
        {
            for(int i = 0; i < n; i ++)
            {
                int theta = i * 6283 / n;

                Vector2Int dir = new Vector2Int(TrigTable.Cos(theta), TrigTable.Sin(theta));
                int radius = WorldConstants.SPACE_SCALE / 50;

                //calcule la distance min du centre pour que les fragments ne se touchent pas
                int alpha = 6283 / n;
                int dist = 2 * radius * TrigTable.Cos(alpha) / TrigTable.Sin(alpha) * 1100 / 1000;


                Bullet bullet = new Bullet(this.Position + dir * dist / 1000, dir, speed, radius);
                gameplayManager.Instantiate(bullet);
            }
            Blowed?.Invoke();
        }

    }
}