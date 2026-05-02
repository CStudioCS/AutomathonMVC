using Automathon.Game.View;
using Automathon.Game.World;
using UnityEngine;



namespace Automathon.Game.GrenadeSystem
{
    public class GrenadeView : EntityView<Grenade>
    {
        public override void Initialize(Grenade grenade)
        {
            base.Initialize(grenade);
            //Normalement la vue doit être directe adaptée à la grenade, on est pas censé le faire au runtime :/
            transform.localScale = Vector3.one * 2 * grenade.CircleCollider.Radius / (float)WorldConstants.SPACE_SCALE;
        }
    }

}