using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public class CollisionEvent
    {
        public class ContactData
        {
            /// <summary>Unit normal pointing from Self towards Other.</summary>
            public FixedVector2 Normal { get; private set; }
            public FixedVector2 Position { get; private set; }
            public Fixed Penetration { get; private set; }

            public ContactData(FixedVector2 position, FixedVector2 normal, Fixed penetration)
            {
                Position = position;
                Normal = normal;
                Penetration = penetration;
            }
        }

        public Collider Self { get; private set; }
        public Collider Other { get; private set; }
        public List<ContactData> Contacts = new();
        public FixedVector2 AverageNormal;
        public Fixed MaxPenetration;

        public CollisionEvent(Collider self, Collider other)
        {
            Self = self;
            Other = other;
        }
    }
}
