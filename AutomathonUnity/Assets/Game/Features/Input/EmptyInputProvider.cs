namespace Automathon.Game.Input
{
    public class EmptyInputProvider : InputProvider
    {
        public override FixedVector2 GetAimingDir() => FixedVector2.Zero;
        public override bool ShouldDash() => false;

        public override bool ShouldMissile() => false;

        public override FixedVector2 GetMovementDir() => FixedVector2.Zero;

        public override bool ShouldShield() => false;

        public override bool ShouldShoot() => false;
    }
}