using System.Collections.Generic;
using UnityEngine;

namespace Automathon.Engine.Physics
{
    public class CollisionEvent
    {
        public Collider Self;
        public Collider Other;
        public List<ContactPoint> ContactPoints; // all points for this pair
        public Vector2Int AverageNormal;         // convenient aggregate
        public int MaxPenetration;
    }
}
