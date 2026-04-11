using Automathon.Engine;
using Automathon.Engine.Physics;

namespace Automathon.Game.Tank
{
    public class Tank : Entity
    {
        public Tank(Vector2Int position) : base(position)
        {
            AddComponent(new BoxCollider(Vector2Int.Zero, 20, 20,  0));
            AddComponent(new Rigidbody());
        }

        public override void Update()
        {
            base.Update();

            
        }
    }
}
