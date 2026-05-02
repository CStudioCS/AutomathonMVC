using Automathon.Game.ShieldSystem;
using Automathon.Game.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Automathon.Game.TankSystem
{
    public class TankView : EntityView<Tank>
    {
        public PlayerInput PlayerInput; //Can't make into a private set cuz it needs to be set in the inspector
        [SerializeField] private ShieldAbilityView shieldAbilityView;

        public override void Initialize(Tank tank)
        {
            base.Initialize(tank);
            //initialization of tank view implies initialization of tank abilities' views
            shieldAbilityView.Initialize(tank.ShieldAbility);
        }
    }

}