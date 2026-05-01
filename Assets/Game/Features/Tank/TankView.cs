using Automathon.Game.ShieldSystem;
using Automathon.Game.Utility;
using Automathon.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.TankSystem
{
    public class TankView : MonoBehaviour
    {
        private Tank tank;
        public PlayerInput PlayerInput; //Can't make into a private set cuz it needs to be set in the inspector
        [SerializeField] private ShieldAbilityView shieldAbilityView;

        public void Initialize(Tank tank)
        {
            this.tank = tank;
            //initialization of tank view implies initialization of tank abilities' views
            shieldAbilityView.Initialize(tank.ShieldAbility);
        }

        private void LateUpdate()
        {
            transform.position = tank.Position.ToVector2Scaled();
            transform.rotation = ViewMath.MilliRadRotationToQuaternion(tank.BoxCollider.RotationMillirad);
        }
    }

}