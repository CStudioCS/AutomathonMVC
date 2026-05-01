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
        public ShieldAbility ShieldAbility { get; private set; }
        public GameplayManager GameplayManager { get; private set; }
        public Vector2Int LastMilliDirection { get; private set; }//should be defined at the collider level?

        public BoxCollider BoxCollider { get; private set; }

        public Tank(Vector2Int position, IInputProvider inputProvider, GameplayManager gameplayManager) : base(position)
        {
            this.inputProvider = inputProvider;
            BoxCollider = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
            Rigidbody = new Rigidbody(BoxCollider, 1000, 500, 200);
            ShieldAbility = new ShieldAbility(this, inputProvider.ShouldShield, gameplayManager);

            Initialize(BoxCollider, Rigidbody, ShieldAbility);
            GameplayManager = gameplayManager;
        }

        public override void Update()
        {
            base.Update();

            Vector2Int movementInput = inputProvider.GetMilliMovementDir();

            Rigidbody.Velocity = movementInput * SPEED / 1000;

            Vector2Int directionInput = inputProvider.GetMilliAimingDir();

            base.RotationMilli = movementInput.CalculateAngleMilliRad();//change for directionInput instead of movementInput

            if ((movementInput.X, movementInput.Y) != (0, 0))
            {
                LastMilliDirection = movementInput;
            }
        }
    }
}
