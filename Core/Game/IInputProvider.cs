namespace Automathon.Game.Input
{
    public interface IInputProvider
    {
        public Vector2Int GetMilliMovementDir();
        public Vector2Int GetMilliAimingDir();
        public bool ShouldShoot();
        public bool ShouldShield();
        public bool ShouldMissile();
        public bool ShouldDash();
    }
}
