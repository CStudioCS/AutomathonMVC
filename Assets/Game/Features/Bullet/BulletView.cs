using Automathon.Game.View;
using Automathon.Game.World;
using UnityEngine;


namespace Automathon.Game.BulletSystem
{
    public class BulletView : EntityView<Bullet>
    {
        public override void Initialize(Bullet bullet)
        {
            base.Initialize(bullet);
            //Normalement la vue doit être directe adaptée à la grenade, on est pas censé le faire au runtime :/
            transform.localScale = Vector3.one * 2 * bullet.CircleCollider.Radius / (float)WorldConstants.SPACE_SCALE;
        }
    }
}