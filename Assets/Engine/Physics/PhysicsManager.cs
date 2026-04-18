using System.Collections.Generic;
using UnityEngine;

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
            List<Contact> newContacts = new();

            void WarmStart(Contact contact)
            {
                Contact previousContact = contacts.Find((foundContact) => (foundContact.Reference == contact.Reference && foundContact.Incident == contact.Incident) || (foundContact.Reference == contact.Incident && foundContact.Incident == contact.Reference));
                
                if (previousContact != null)
                {
                    contact.Pn = previousContact.Pn;
                    contact.Pt = previousContact.Pt;
                }
            }

            void HandleBoxBoxContact(Rigidbody rigidbody1, BoxCollider boxCollider1, Rigidbody rigidbody2, BoxCollider boxCollider2)
            {
                Collision.BoxContact boxContact = Collision.BoxBoxClipping(boxCollider1, boxCollider2);
                Rigidbody referenceBody = boxContact.Reference == boxCollider1 ? rigidbody1 : rigidbody2;
                Rigidbody incidentBody = boxContact.Reference == boxCollider1 ? rigidbody2 : rigidbody1;

                void AddContact(Vector2Int position)
                {
                    if (!boxContact.Reference.Contains(boxContact.ClippedIncidentFaceCoord1))
                        return;

                    Contact contact = new Contact(referenceBody, incidentBody, boxContact.ClippedIncidentFaceCoord1, boxContact.Normal, boxContact.PenetrationMilli);

                    WarmStart(contact);
                    newContacts.Add(contact);
                }

                AddContact(boxContact.ClippedIncidentFaceCoord1);
                AddContact(boxContact.ClippedIncidentFaceCoord2);
            }

            void HandleBoxCircleContact(BoxCollider boxCollider, CircleCollider circleCollider)
            {

            }

            void HandleCircleCircleContact(CircleCollider circleCollider1, CircleCollider circleCollider2)
            {

            }


            
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                for (int j = i + 1; j < rigidbodies.Count; j++)
                {
                    Rigidbody rigidbody = rigidbodies[i];
                    Rigidbody otherRigidbody = rigidbodies[j];

                    if (rigidbody.InvMassMilli == 0 && otherRigidbody.InvMassMilli == 0)
                        continue;

                    if (rigidbody.Collider is BoxCollider b1 && otherRigidbody.Collider is BoxCollider b2)
                        HandleBoxBoxContact(rigidbody, b1, otherRigidbody, b2);
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
