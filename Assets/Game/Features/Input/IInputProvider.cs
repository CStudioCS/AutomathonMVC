namespace Automathon.Game.Input
{
    public interface IInputProvider
    {
        public Vector2Int GetMilliMovementDir();
        public bool ShouldShoot();
        public bool ShouldShield();
        public bool ShouldGrenade();
        public bool ShouldDash();
    }
}
