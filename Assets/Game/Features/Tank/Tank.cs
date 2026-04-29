using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;
using Automathon.Game.ShieldSystem;

namespace Automathon.Game.TankSystem
{
    public class Tank : Entity
    {
        private const int SPEED = 4000;

        private IInputProvider inputProvider;
        public Rigidbody Rigidbody { get; private set; }
        public ShieldAbility shieldAbility { get; private set; }
        public GameplayManager gameplayManager { get; private set; }
        public Vector2Int lastMilliDirection { get; private set; }//should be defined at the collider level?

        public Tank(Vector2Int position, IInputProvider inputProvider, GameplayManager gameplayManager) : base(position)
        {
            this.inputProvider = inputProvider;
            Collider coll = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
            Rigidbody = new Rigidbody(coll);
            shieldAbility = new ShieldAbility(this, inputProvider.ShouldShield, gameplayManager);

            Initialize(coll, Rigidbody, shieldAbility);
            this.gameplayManager = gameplayManager;
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = inputProvider.GetMilliMovementDir();

            Rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int directionInput = inputProvider.GetMilliAimingDir();

            ((BoxCollider)Rigidbody.Collider).RotationMillirad = movementInput.CalculateAngleMilliRad();//change for directionInput instead of movementInput

            if ((movementInput.X, movementInput.Y) != (0, 0))
            {
                lastMilliDirection = movementInput;
            }
        }
    }
}
