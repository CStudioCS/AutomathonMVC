using Automathon.Game.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.TankSystem
{
    public class TankView : MonoBehaviour
    {
        public Tank Tank { get; set; }
        public PlayerInput PlayerInput; //Can't make into a private set cuz it needs to be set in the inspector

        public void Initialize(Tank tank)
        {
            this.Tank = tank;
        }

        private void LateUpdate()
        {
            transform.position = Tank.Position.ToVector2Scaled();
            transform.rotation = Quaternion.Euler(0, 0, ((Automathon.Engine.Physics.BoxCollider)tank.Rigidbody.Collider).RotationMillirad * 1000 * Mathf.Rad2Deg);
        }
    }

}