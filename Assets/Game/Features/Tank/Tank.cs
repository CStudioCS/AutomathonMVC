using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.BulletSystem;
using Automathon.Game.GrenadeSystem;
using Automathon.Game.HealthSystem;
using Automathon.Game.Input;
using Automathon.Game.ShieldSystem;

namespace Automathon.Game.TankSystem
{
    public class Tank : Entity
    {
        private const int SPEED = 4000;
        public const int MAX_HEALTH = 1000;

        public IInputProvider InputProvider { get; private set; }
        private Rigidbody rigidbody;

        public Vector2Int LastMilliDirection { get; private set; }
        public bool IsReady { get; set; }

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            InputProvider = inputProvider;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
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

            Vector2Int directionInput = InputProvider.GetMilliAimingDir();
            directionInput.NormalizeAtScale(1000);

            if (movementInput != Vector2Int.Zero)
            {
                RotationMilli = movementInput.CalculateAngleMilliRad(); //change for directionInput instead of movementInput when controllers are mainly used
                rigidbody.AngularVelocityMilli = 0;
            }

            if ((movementInput.X, movementInput.Y) != (0, 0))
                LastMilliDirection = movementInput;
        }

        private void Death()
        {
            //The actual details of this will be made by Cedric
            GameplayManager.Destroy(this);
        }
    }
}
