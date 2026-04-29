using Automathon.Game.ShieldSystem;
using Automathon.Game.Utility;
using UnityEngine;

public class ShieldView : MonoBehaviour
{
    private Shield shield;

    internal void Initialize(Shield shield)
    {
        this.shield = shield;
    }

    private void LateUpdate()
    {
        transform.position = shield.Position.ToVector2Scaled();
        transform.rotation = Quaternion.Euler(0, 0, ((Automathon.Engine.Physics.BoxCollider)shield.Rigidbody.Collider).RotationMillirad * Mathf.Rad2Deg / 1000);
    }
}
