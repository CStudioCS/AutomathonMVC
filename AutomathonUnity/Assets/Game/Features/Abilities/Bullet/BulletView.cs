using UnityEngine;

namespace Automathon.Game
{
    public class BulletView : EntityView<Bullet>
    {
        public override void Initialize(Bullet entity)
        {
            base.Initialize(entity);
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color32(255, 68, 204, 255); // #ff44cc pink
            entity.HitWall += () => SoundManager.instance.PlaySound("HitWall");
            entity.HitTank += () => SoundManager.instance.PlaySound("HitTank");
        }
    }
}