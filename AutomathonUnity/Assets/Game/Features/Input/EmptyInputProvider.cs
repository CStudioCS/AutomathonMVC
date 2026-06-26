namespace Automathon.Game.Input
{
    public class EmptyInputProvider : InputProvider
    {
        public override Vector2Int GetMilliAimingDir() => Vector2Int.Zero;
        public override bool ShouldDash() => false;

        public override bool ShouldMissile() => false;

        public override Vector2Int GetMilliMovementDir() => Vector2Int.Zero;

        public override bool ShouldShield() => false;

        public override bool ShouldShoot() => false;
    }
}