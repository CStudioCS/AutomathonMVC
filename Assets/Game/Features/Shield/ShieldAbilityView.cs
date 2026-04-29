using Automathon.Game.ShieldSystem;
using UnityEngine;

public class ShieldAbilityView : MonoBehaviour
{
    private ShieldAbility shieldAbility;
    [SerializeField] private ShieldView shieldPrefab;
    //create a list of active shields ?

    public void Initialize(ShieldAbility shieldAbility)
    {
        this.shieldAbility = shieldAbility;
        this.shieldAbility.OnShieldActivated += OnShieldActivated;
    }

    private void OnShieldActivated(Shield shield)
    {
        Vector2 shieldPosition = new Vector2(shieldAbility.tank.Position.X, shieldAbility.tank.Position.Y) / 1000f;
        Quaternion shieldRotation = Quaternion.Euler(0, 0, shield.boxCollider.RotationMillirad * Mathf.Rad2Deg / 1000f);
        ShieldView shieldInstance = Instantiate(shieldPrefab, shieldPosition, shieldRotation);
        shieldInstance.Initialize(shield);
    }

}
