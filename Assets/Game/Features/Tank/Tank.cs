using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.Input;

namespace Automathon.Game.Tank
{
    public class Tank : Entity
    {
        private const int SPEED = 4000;

        private IInputProvider inputProvider;
        private Rigidbody rigidbody;

        public Tank(Vector2Int position, IInputProvider inputProvider) : base(position)
        {
            this.inputProvider = inputProvider;
            AddComponent(new BoxCollider(Vector2Int.Zero, 1000, 1000, 0));
            rigidbody = AddComponent(new Rigidbody());
        }

        public override void Update()
        {
            base.Update();

            rigidbody.Velocity = inputProvider.GetMilliMovementDir() * SPEED / 1000;
        }
    }
}
