using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;

namespace Automathon.Game
{
    public class Tank : Entity
    {
        private const int SIZE = 700;
        private const int SPEED = 7000;
        public const int MAX_HEALTH = 1000;

        public BulletAbility BulletAbility;
        public ShieldAbility ShieldAbility;
        public GrenadeAbility GrenadeAbility;
        public MachineGunAbility MachineGunAbility;
        public DashAbility DashAbility;
        public Health Health;

        public IInputProvider InputProvider { get; private set; }
        private Rigidbody rigidbody;

        public Vector2Int LastMilliDirection { get; private set; } = new Vector2Int(1000, 0);
        public Vector2Int LastMovingMilliDirection { get; private set; } = new Vector2Int(1000, 0);
        public bool IsReady { get; set; }
        public bool IsDashing { get; set; }

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            InputProvider = inputProvider;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, SIZE, SIZE, 0);
            rigidbody = new Rigidbody(boxCollider, 1000, 500, 200);

            Initialize(
                boxCollider,
                rigidbody,
                BulletAbility = new BulletAbility(inputProvider.ShouldShoot), //i'm using fancy new syntax mwahahaha
                GrenadeAbility = new GrenadeAbility(inputProvider.ShouldGrenade),
                DashAbility = new DashAbility(inputProvider.ShouldDash),
                //ShieldAbility = new ShieldAbility(inputProvider.ShouldShield),
                MachineGunAbility = new MachineGunAbility(10, 500, 3000, inputProvider.ShouldShield),
                Health = new Health(MAX_HEALTH, false, Death)
                );
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = InputProvider.GetMilliMovementDir();

            // Only update velocity if not dashing (dash manages its own velocity)
            if (!IsDashing)
                rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int aimingInput = InputProvider.GetMilliAimingDir();

            /*if (InputProvider is PlayerInputProvider playerInputProvider && playerInputProvider.PlayerControls == PlayerInputProvider.PlayerControlsType.RightKeyboard)
                aimingInput = new Vector2Int(aimingInput.X - Position.X, aimingInput.Y - Position.Y);
            */

            if (movementInput != Vector2Int.Zero)
            {
                LastMovingMilliDirection = movementInput;
                RotationMilli = movementInput.CalculateAngleMilliRad();
                rigidbody.AngularVelocityMilli = 0;

                if (InputProvider is PlayerInputProvider p && (p.ControlsType == PlayerInputProvider.PlayerControlsType.LeftKeyboard || p.ControlsType == PlayerInputProvider.PlayerControlsType.RightKeyboard))
                    LastMilliDirection = movementInput;
            }

            if (aimingInput != Vector2Int.Zero)
                LastMilliDirection = aimingInput;
        }

        private void Death()
        {
            //The actual details of this will be made by whoever handles Gameplay end
            GameplayManager.Destroy(this);
        }
    }
}
