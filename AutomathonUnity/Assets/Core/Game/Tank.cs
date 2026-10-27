using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;

namespace Automathon.Game
{
    public class Tank : Entity
    {
        public class TankState : State
        {
            public Fixed Width;
            public Fixed Height;
            public Fixed Radius;
            public Fixed Rotation;
            public FixedVector2 Velocity;
            public FixedVector2 AimingDirection;

            public Fixed Health;

            public int ShieldCooldownFramesLeft;
            public int MissileCooldownFramesLeft;
            public int MachineGunCooldownFramesLeft;
            public int DashCooldownFramesLeft;
        }

        public enum TeamType { Red, Green }

        public static readonly Fixed TANK_HEIGHT = 0.838;
        public static readonly Fixed TANK_WIDTH = 1.138;
        /// <summary>Max speed in units per second.</summary>
        public static readonly Fixed SPEED = 7;
        public static readonly Fixed MAX_HEALTH = 1000;
        public static readonly Fixed SPAWN_DISTANCE_FROM_TANK = 0.7;
        // Circular hitbox: diameter = the smaller side of the sprite's rectangle (min(W, H)).
        public static readonly Fixed RADIUS = FixedMath.Min(TANK_WIDTH, TANK_HEIGHT) / 2;

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

        /// <summary>Last non-zero aiming direction (unit vector).</summary>
        public FixedVector2 LastDirection { get; private set; } = FixedVector2.Right;
        /// <summary>Last non-zero movement direction.</summary>
        public FixedVector2 LastMovingDirection { get; private set; } = FixedVector2.Right;
        public bool IsReady { get; set; }
        public bool IsDashing { get; set; }

        public Tank(TeamType team, FixedVector2 position, InputProvider inputProvider) : base(position)
        {
            InputProvider = inputProvider;
            Team = team;

            CircleCollider circleCollider = new CircleCollider(FixedVector2.Zero, RADIUS);
            circleCollider.Layer = CollisionLayer.Tank;
            Rigidbody = new Rigidbody(circleCollider, invMass: 1, invInertia: 0.5, friction: 0);

            Initialize(
                circleCollider,
                Rigidbody,
                inputProvider,
                MachineGunAbility = new MachineGunAbility(10, 0.5, inputProvider.ShouldShoot),
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
                Rigidbody.Velocity = FixedVector2.Zero;
                Rigidbody.AngularVelocity = Fixed.Zero;
                return;
            }

            base.Update();

            FixedVector2 movementInput = InputProvider.GetMovementDir().ClampMagnitude(1);

            // Only update velocity if not dashing (dash manages its own velocity)
            if (!IsDashing)
                Rigidbody.Velocity = movementInput * SPEED;

            FixedVector2 aimingInput = InputProvider.GetAimingDir();

            if (movementInput != FixedVector2.Zero)
            {
                LastMovingDirection = movementInput;
                Rotation = movementInput.Angle();
                Rigidbody.AngularVelocity = Fixed.Zero;
            }

            if (aimingInput != FixedVector2.Zero)
                LastDirection = aimingInput.Normalized();
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
                Radius = RADIUS,
                Rotation = this.Rotation,
                Velocity = Rigidbody.Velocity,
                AimingDirection = LastDirection,
                Health = Health.CurrentHealth,
                DashCooldownFramesLeft = DashAbility.FramesOfCooldownLeft,
                MachineGunCooldownFramesLeft = MachineGunAbility.FramesOfCooldownLeft,
                MissileCooldownFramesLeft = MissileAbility.FramesOfCooldownLeft,
                ShieldCooldownFramesLeft = ShieldAbility.FramesOfCooldownLeft,
            };
    }
}
