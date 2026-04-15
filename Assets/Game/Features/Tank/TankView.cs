using Automathon.Game.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.TankSystem
{
    public class TankView : MonoBehaviour
    {
        private Tank tank;
        public PlayerInput playerInput { get; private set; }

        public void Initialize(Tank tank)
        {
            this.tank = tank;
        }

        private void LateUpdate()
        {
            transform.position = tank.Position.ToVector2Scaled();
            if (tank.TryGetComponent<Automathon.Engine.Physics.BoxCollider>(out var b))
                transform.rotation = Quaternion.Euler(0, 0, b.RotationMillirad * 1000 * Mathf.Rad2Deg);
        }
    }

}