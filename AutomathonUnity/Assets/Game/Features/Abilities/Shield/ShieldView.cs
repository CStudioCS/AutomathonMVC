namespace Automathon.Game
{
    public class ShieldView : EntityView<Shield>
    {
        public override void Initialize(Shield entity)
        {
            base.Initialize(entity);
            SoundManager.instance.PlaySound("Shield");
            ScreenGlitch.Trigger();
            // The neon outline is authored on the Shield prefab (Follow = true, so it self-updates).
        }
    }
}

