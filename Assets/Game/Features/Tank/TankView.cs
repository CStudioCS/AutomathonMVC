using Automathon.Game.View;
using UnityEngine.InputSystem;

namespace Automathon.Game.TankSystem
{
    public class TankView : EntityView<Tank>
    {
        public PlayerInput PlayerInput; //Can't make into a private set cuz it needs to be set in the inspector
    }

}