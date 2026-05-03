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

        public int Health = MAX_HEALTH;

        private IInputProvider inputProvider;
        private Rigidbody rigidbody;
        public Vector2Int LastMilliDirection { get; private set; }

        private BulletAbility bulletAbility;
        private ShieldAbility shieldAbility;
        private GrenadeAbility grenadeAbility;

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            this.inputProvider = inputProvider;

            BoxCollider boxCollider = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
            rigidbody = new Rigidbody(boxCollider, 1000, 500, 200);

            bulletAbility = new BulletAbility(inputProvider.ShouldShoot);
            shieldAbility = new ShieldAbility(inputProvider.ShouldShield);
            grenadeAbility = new GrenadeAbility(inputProvider.ShouldGrenade);

            Initialize(boxCollider, rigidbody, bulletAbility, shieldAbility, grenadeAbility);
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = inputProvider.GetMilliMovementDir();

            rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int directionInput = inputProvider.GetMilliAimingDir();

            RotationMilli = movementInput.CalculateAngleMilliRad();//change for directionInput instead of movementInput

            if ((movementInput.X, movementInput.Y) != (0, 0))
                LastMilliDirection = movementInput;
        }

        public void Damage(int damage)
        {
            Health -= damage;

            if (Health < 0)
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
