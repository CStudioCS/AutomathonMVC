namespace Automathon.Game
{
    public class BulletView : EntityView<Bullet>
    {
        public override void Initialize(Bullet entity)
        {
            base.Initialize(entity);
            SoundManager.instance.PlaySound("FireBullet");
        }
    }
}