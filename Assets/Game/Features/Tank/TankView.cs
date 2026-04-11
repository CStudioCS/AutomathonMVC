using Automathon;
using Automathon.Game.Tank;
using Automathon.Game.Utility;
using UnityEngine;

public class TankView : MonoBehaviour
{
    private Tank tank;

    public void Initialize(Tank tank)
    {
        this.tank = tank;
    }

    private void LateUpdate()
    {
        transform.position =  tank.Position.ToV2() / GameplayConstants.SpaceScale;
        Automathon.Debug.Log($"Tank position: {transform.position}");
    }
}
