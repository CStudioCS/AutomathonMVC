using System;
using System.Collections.Generic;
using UnityEngine;

namespace Automathon.Engine.Physics
{
    public static class PhysicsManager
    {
        private static readonly List<Rigidbody> rigidbodies = new();
        private static List<Contact> contacts = new();

        public static int substeps = 10;

        public static int Iterations = 10;
        public static int KBias = 20;
        public static int SlopPenetration = 10;

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
                throw new NotImplementedException();
            }

            void HandleCircleCircleContact(CircleCollider circleCollider1, CircleCollider circleCollider2)
            {
                throw new NotImplementedException();
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
                }
            }

            contacts = newContacts;
        }

        private static void ApplyForces()
        {
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.Velocity += rb.InvMassMilli * rb.Forces / (GameplayConstants.FRAMERATE * 1000);
                rb.AngularVelocityMilli += rb.InvIMilli * rb.TorqueMilli / (GameplayConstants.FRAMERATE * 1000 * 1000);
            }
        }

        public static void Step()
        {
            BroadPhase();

            ApplyForces();

            foreach (Contact contact in contacts)
                contact.PreStep();

            //Loop to solve all constraints
            for (int i = 0; i < substeps; i++)
            {
                foreach (Contact contact in contacts)
                    contact.ApplyImpulse();
            }


            foreach (Rigidbody rb in rigidbodies)
            {
                Debug.Log(rb.Velocity);
                rb.ParentEntity.Position += rb.Velocity / GameplayConstants.FRAMERATE;
                rb.ParentEntity.RotationMilli += rb.AngularVelocityMilli / GameplayConstants.FRAMERATE;

                //Putting it between -pi and +pi
                int twoPiApprox = 6282; // 2 * 3.1415 * 1000;
                rb.ParentEntity.RotationMilli = rb.ParentEntity.RotationMilli % twoPiApprox;
                if (rb.ParentEntity.RotationMilli > twoPiApprox / 2)
                    rb.ParentEntity.RotationMilli -= twoPiApprox;

                rb.Forces = Vector2Int.Zero;
                rb.TorqueMilli = 0;
            }
        }
    }
}
