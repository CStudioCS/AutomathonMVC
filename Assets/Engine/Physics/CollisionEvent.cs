using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public class CollisionEvent
    {
        public class ContactData
        {
            public Vector2Int Normal { get; private set; }
            public Vector2Int Position { get; private set; }
            public int Penetration { get; private set; }

            public ContactData(Vector2Int position, Vector2Int normal, int penetration)
            {
                Position = position;
                Normal = normal;
                Penetration = penetration;
            }
        }

        public Collider Self { get; private set; }
        public Collider Other { get; private set; }
        public List<ContactData> Contacts = new();
        public Vector2Int AverageNormal;
        public int MaxPenetration;

        public CollisionEvent(Collider self, Collider other)
        {
            Self = self;
            Other = other;
        }

    }
}
