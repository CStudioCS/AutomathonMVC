namespace Automathon.Game
{
    public class ShieldView : EntityView<Shield>
    {
        public override void Initialize(Shield entity)
        {
            base.Initialize(entity);
            SoundManager.instance.PlaySound("Shield");
        }
    }
}

