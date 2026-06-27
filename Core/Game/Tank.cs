using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;

namespace Automathon.Game
{
    public class Tank : Entity
    {
        public class TankState : State
        {
            public int Width;
            public int Height;
            public Vector2Int Velocity;

            public int Health;

            public int ShieldCooldownFramesLeft;
            public int MissileCooldownFramesLeft;
            public int MachineGunCooldownFramesLeft;
            public int DashCooldownFramesLeft;
        }

        public enum TeamType { Red, Green }

        private const int TANK_HEIGHT = 838;
        private const int TANK_WIDTH = 1138;
        private const int SPEED = 7000;
        public const int MAX_HEALTH = 1000;
        public const int SPAWN_DISTANCE_FROM_TANK = 700;

        public TeamType Team;
        //public BulletAbility BulletAbility;
        //public GrenadeAbility GrenadeAbility;
        public ShieldAbility ShieldAbility;
        public MissileAbility MissileAbility;
        public MachineGunAbility MachineGunAbility;
        public DashAbility DashAbility;
        public Health Health;

        public InputProvider InputProvider { get; private set; }
        public Rigidbody Rigidbody;

        public Vector2Int LastMilliDirection { get; private set; } = new Vector2Int(1000, 0);
        public Vector2Int LastMovingMilliDirection { get; private set; } = new Vector2Int(1000, 0);
        public bool IsReady { get; set; }
        public bool IsDashing { get; set; }

        public Tank(TeamType team, Vector2Int position, InputProvider inputProvider) : base(position)
        {
            InputProvider = inputProvider;
            Team = team;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, TANK_WIDTH, TANK_HEIGHT, 0);
            boxCollider.Layer = CollisionLayer.Tank;
            Rigidbody = new Rigidbody(boxCollider, 1000, 500, 200);

            Initialize(
                boxCollider,
                Rigidbody,
                inputProvider,
                MachineGunAbility = new MachineGunAbility(10, 500, inputProvider.ShouldShoot),
                MissileAbility = new MissileAbility(inputProvider.ShouldMissile),
                ShieldAbility = new ShieldAbility(inputProvider.ShouldShield),
                DashAbility = new DashAbility(inputProvider.ShouldDash),
                //BulletAbility = new BulletAbility(inputProvider.ShouldShoot), //i'm using fancy new syntax mwahahaha
                //GrenadeAbility = new GrenadeAbility(inputProvider.ShouldGrenade),
                Health = new Health(MAX_HEALTH, false, Death)
                );
        }

        public override void Update()
        {
            if (GameplayManager.State != GameplayManager.GameplayState.Game)
            {
                Rigidbody.Velocity = Vector2Int.Zero;
                Rigidbody.AngularVelocityMilli = 0;
                return;
            }

            base.Update();

            Vector2Int movementInput = InputProvider.GetMilliMovementDir();

            // Only update velocity if not dashing (dash manages its own velocity)
            if (!IsDashing)
                Rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int aimingInput = InputProvider.GetMilliAimingDir();

            /*if (InputProvider is PlayerInputProvider playerInputProvider && playerInputProvider.PlayerControls == PlayerInputProvider.PlayerControlsType.RightKeyboard)
                aimingInput = new Vector2Int(aimingInput.X - Position.X, aimingInput.Y - Position.Y);
            */


            if (movementInput != Vector2Int.Zero)
            {
                LastMovingMilliDirection = movementInput;
                RotationMilli = Atan2Int.Atan2(movementInput.X, movementInput.Y);
                Rigidbody.AngularVelocityMilli = 0;

                /*if (InputProvider is PlayerInputProvider p && (p.ControlsType == PlayerInputProvider.PlayerControlsType.LeftKeyboard || p.ControlsType == PlayerInputProvider.PlayerControlsType.RightKeyboard))
                    LastMilliDirection = movementInput;*/
            }

            if (aimingInput != Vector2Int.Zero)
                LastMilliDirection = aimingInput;
        }

        private void Death()
        {
            GameplayManager.Destroy(this);
            GameplayManager.EndGame(Team);
        }

        public override State GetState()
            => new TankState()
            {
                Position = this.Position,
                Width = TANK_WIDTH,
                Height = TANK_HEIGHT,
                Velocity = Rigidbody.Velocity,
                Health = Health.CurrentHealth,
                DashCooldownFramesLeft = DashAbility.FramesOfCooldownLeft,
                MachineGunCooldownFramesLeft = MachineGunAbility.FramesOfCooldownLeft,
                MissileCooldownFramesLeft = MissileAbility.FramesOfCooldownLeft,
                ShieldCooldownFramesLeft = ShieldAbility.FramesOfCooldownLeft,
            };
    }
}
