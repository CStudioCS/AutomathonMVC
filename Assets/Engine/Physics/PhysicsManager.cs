using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public static class PhysicsManager
    {
        private static readonly List<Rigidbody> rigidbodies = new();
        private static List<Contact> contacts = new();

        public static int Iterations = 10;
        public static float KBias = 0.2f;
        public static float SlopPenetration = 0.1f;

        static PhysicsManager()
        {
            Rigidbody.Added += OnRigidbodyAdded;
            Rigidbody.Removed += OnRigidbodyRemoved;
        }

        public static void Dispose()
        {
            Rigidbody.Added -= OnRigidbodyAdded;
            Rigidbody.Removed -= OnRigidbodyRemoved;
        }

        private static void OnRigidbodyAdded(Rigidbody rigidbody)
            => rigidbodies.Add(rigidbody);

        private static void OnRigidbodyRemoved(Rigidbody rigidbody)
            => rigidbodies.Remove(rigidbody);

        private static void BroadPhase()
        {
            void HandleBoxBoxContact(BoxCollider boxCollider1, BoxCollider boxCollider2)
            {
                Collision.BoxContact boxContact = Collision.BoxBoxClipping(boxCollider1, boxCollider2);

                List<Contact> contacts = new();
                if (boxContact.Reference.Collider.Collide(boxContact.ClippedIncidentFace1))
                    contacts.Add(new Contact(boxContact.Reference.ParentEntity.GetComponent<Rigidbody>(), boxContact.Incident, boxContact.ClippedIncidentFace1, boxContact.Normal, boxContact.Penetration));
                if (boxContact.Reference.Collider.Collide(boxContact.ClippedIncidentFace2))
                    contacts.Add(new Contact(boxContact.Reference.ParentEntity.GetComponent<Rigidbody>(), boxContact.Incident, boxContact.ClippedIncidentFace2, boxContact.Normal, boxContact.Penetration));
            }

            void HandleBoxCircleContact(BoxCollider boxCollider, CircleCollider circleCollider)
            {

            }

            void HandleCircleCircleContact(CircleCollider circleCollider1, CircleCollider circleCollider2)
            {

            }


            List<Contact> newContacts = new();
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                for (int j = i + 1; j < rigidbodies.Count; j++)
                {
                    Rigidbody rigidbody = rigidbodies[i];
                    Rigidbody otherRigidbody = rigidbodies[j];

                    if (rigidbody.InvMassMilli == 0 && otherRigidbody.InvMassMilli == 0)
                        continue;

                    if (rigidbody.Collider is BoxCollider b1 && otherRigidbody.Collider is BoxCollider b2)
                        HandleBoxBoxContact(b1, b2);
                    else if ((rigidbody.Collider is BoxCollider b3 && otherRigidbody.Collider is CircleCollider c1))
                        HandleBoxCircleContact(b3, c1);
                    else if (rigidbody.Collider is CircleCollider c2 && otherRigidbody.Collider is BoxCollider b4)
                        HandleBoxCircleContact(b4, c2);
                    else if (rigidbody.Collider is CircleCollider c3 && otherRigidbody.Collider is CircleCollider c4)
                        HandleCircleCircleContact(c3, c4);

                    //TODO: Implement Warm start



                    /*Contact c = contacts.Find((c2) => (c2.Reference == rigidbody && c2.Incident == otherRigidbody) || (c2.Reference == rigidbody && c2.Incident == otherRigidbody));
                    if (c != null)
                    {
                        separate[0].Pn = c.Pn; //Super ghetto
                        separate[0].Pt = c.Pt;
                    }

                    newContacts.AddRange(separate);*/
                }
            }

            contacts = newContacts;
        }

        public static void Step()
        {
            BroadPhase();
        }
    }
}
