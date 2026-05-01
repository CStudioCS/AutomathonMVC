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
        public Shield shield { get; private set; }

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            this.inputProvider = inputProvider;
            Collider coll = new BoxCollider(Vector2Int.Zero, 500, 500, 0);
            Rigidbody = new Rigidbody(coll, 1000, 500, 200);
            shield = new Shield(inputProvider.ShouldShield);

            Initialize(coll, Rigidbody, shield);
        }

        public override void Update()
        {
            base.Update();

            Rigidbody.Velocity = inputProvider.GetMilliMovementDir() * SPEED / 1000;
        }
    }
}
