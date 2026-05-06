namespace Automathon.Game.Input
{
    public class EmptyInputProvider : IInputProvider
    {
        public Vector2Int GetMilliAimingDir() => Vector2Int.Zero;
        public bool ShouldDash() => false;

        public bool ShouldGrenade() => false;

        public Vector2Int GetMilliMovementDir() => Vector2Int.Zero;

        public bool ShouldShield() => false;

        public bool ShouldShoot() => false;
    }
}