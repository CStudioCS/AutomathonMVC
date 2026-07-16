namespace Automathon.Game
{
    public class BulletView : EntityView<Bullet>
    {
        public override void Initialize(Bullet entity)
        {
            base.Initialize(entity);
            // Bullet colour is authored on the prefab's SpriteRenderer.
            entity.HitWall += () => SoundManager.instance.PlaySound("HitWall");
            entity.HitTank += () => SoundManager.instance.PlaySound("HitTank");
        }
    }
}
