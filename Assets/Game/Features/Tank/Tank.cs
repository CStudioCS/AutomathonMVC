using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.BulletSystem;
using Automathon.Game.GrenadeSystem;
using Automathon.Game.Input;
using Automathon.Game.ShieldSystem;

namespace Automathon.Game.TankSystem
{
    public class Tank : Entity
    {
        private const int SPEED = 4000;
        public const int MAX_HEALTH = 1000;

        private IInputProvider inputProvider;
        private Rigidbody rigidbody;

        public int Health { get; private set; } = MAX_HEALTH;
        public Vector2Int LastMilliDirection { get; private set; }

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            this.inputProvider = inputProvider;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
            rigidbody = new Rigidbody(boxCollider, 1000, 500, 200);

            Initialize(
                boxCollider,
                rigidbody,
                new BulletAbility(inputProvider.ShouldShoot),
                new ShieldAbility(inputProvider.ShouldShield),
                new GrenadeAbility(inputProvider.ShouldGrenade)
                );
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = inputProvider.GetMilliMovementDir();
            rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int directionInput = inputProvider.GetMilliAimingDir();
            directionInput.NormalizeAtScale(1000);

            if (movementInput != Vector2Int.Zero)
            {
                RotationMilli = movementInput.CalculateAngleMilliRad(); //change for directionInput instead of movementInput when controllers are mainly used
                rigidbody.AngularVelocityMilli = 0;
            }

            if ((movementInput.X, movementInput.Y) != (0, 0))
                LastMilliDirection = movementInput;
        }

        public void Damage(int damage)
        {
            Health -= damage;

            if (Health <= 0)
            {
                Health = 0;
                Death();
            }
        }

        private void Death()
        {
            //The actual details of this will be made by Cedric
            GameplayManager.Destroy(this);
        }
    }
}
