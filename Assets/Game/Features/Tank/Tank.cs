using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;

namespace Automathon.Game
{
    public class Tank : Entity
    {
        private const int SIZE = 600;
        private const int SPEED = 6000;
        public const int MAX_HEALTH = 1000;

        public IInputProvider InputProvider { get; private set; }
        private Rigidbody rigidbody;

        public Vector2Int LastMilliDirection { get; private set; }
        public bool IsReady { get; set; }

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            InputProvider = inputProvider;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, SIZE, SIZE, 0);
            rigidbody = new Rigidbody(boxCollider, 1000, 500, 200);

            Initialize(
                boxCollider,
                rigidbody,
                new BulletAbility(inputProvider.ShouldShoot),
                new ShieldAbility(inputProvider.ShouldShield),
                new GrenadeAbility(inputProvider.ShouldGrenade),
                new Health(MAX_HEALTH, false, Death)
                );
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = InputProvider.GetMilliMovementDir();
            rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int aimingInput = InputProvider.GetMilliAimingDir();

            /*if (InputProvider is PlayerInputProvider playerInputProvider && playerInputProvider.PlayerControls == PlayerInputProvider.PlayerControlsType.RightKeyboard)
                aimingInput = new Vector2Int(aimingInput.X - Position.X, aimingInput.Y - Position.Y);
            */

            if (aimingInput != Vector2Int.Zero)
                LastMilliDirection = aimingInput;
            if (movementInput != Vector2Int.Zero)
            {
                RotationMilli = movementInput.CalculateAngleMilliRad();
                rigidbody.AngularVelocityMilli = 0;
            }
        }

        private void Death()
        {
            //The actual details of this will be made by whoever handles Gameplay end
            GameplayManager.Destroy(this);
        }
    }
}
