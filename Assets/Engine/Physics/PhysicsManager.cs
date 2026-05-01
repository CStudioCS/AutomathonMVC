using System;
using System.Collections.Generic;

namespace Automathon.Engine.Physics
{
    public static class PhysicsManager
    {
        private static readonly List<Rigidbody> rigidbodies = new();
        private static List<Contact> contacts = new();

        public static int Substeps = 6;
        public static int KBiasMilli = 200;
        public static int SlopPenetration = 30;

        public static void Initialize()
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

                if (!boxContact.Colliding)
                    return;

                Rigidbody referenceBody = boxContact.Reference == boxCollider1 ? rigidbody1 : rigidbody2;
                Rigidbody incidentBody = boxContact.Reference == boxCollider1 ? rigidbody2 : rigidbody1;

                void AddContact(Vector2Int position)
                {
                    if (!boxContact.Reference.Contains(position))
                        return;

                    Contact contact = new Contact(referenceBody, incidentBody, boxContact.Reference, boxContact.Incident, position, boxContact.Normal, boxContact.Penetration);

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
                rb.AngularVelocityMilli += rb.InvIMicro * rb.TorqueMilli / (GameplayConstants.FRAMERATE * 1000 * 1000) / 1000000;
            }
        }

        private static void DispatchCollisionEvents()
        {
            Dictionary<Collider, Dictionary<Collider, CollisionEvent>> colliderEvents = new();

            void AddToCollisionEvent(Collider self, Collider other, Vector2Int position, Vector2Int normalMilli, int penetration)
            {
                if (!colliderEvents.ContainsKey(self))
                    colliderEvents[self] = new Dictionary<Collider, CollisionEvent>();

                if (!colliderEvents[self].ContainsKey(other))
                    colliderEvents[self][other] = new CollisionEvent(self, other);

                colliderEvents[self][other].Contacts.Add(new CollisionEvent.ContactData(position, normalMilli, penetration));
            }

            foreach (Contact contact in contacts)
            {
                AddToCollisionEvent(contact.ReferenceCollider, contact.IncidentCollider, contact.Position, contact.NormalMilli, contact.Penetration);
                AddToCollisionEvent(contact.IncidentCollider, contact.ReferenceCollider, contact.Position, -contact.NormalMilli, contact.Penetration);
            }

            foreach (Dictionary<Collider, CollisionEvent> collisionEvents in colliderEvents.Values)
            {
                foreach (CollisionEvent collisionEvent in collisionEvents.Values)
                {
                    collisionEvent.AverageNormal = Vector2Int.Zero;

                    collisionEvent.MaxPenetration = 0;
                    foreach (CollisionEvent.ContactData contactData in collisionEvent.Contacts)
                    {
                        collisionEvent.AverageNormal += contactData.Normal;
                        collisionEvent.MaxPenetration = Math.Max(collisionEvent.MaxPenetration, contactData.Penetration);
                    }

                    collisionEvent.AverageNormal = collisionEvent.AverageNormal / collisionEvent.Contacts.Count;

                    collisionEvent.Self.OnCollision?.Invoke(collisionEvent);
                }
            }
        }

        public static void Step()
        {
            foreach (Rigidbody rigidbody in rigidbodies)
                rigidbody.Collider.PhysicsUpdate();

            BroadPhase();

            ApplyForces();

            foreach (Contact contact in contacts)
                contact.PreStep();

            //Loop to solve all constraints
            for (int i = 0; i < Substeps; i++)
            {
                foreach (Contact contact in contacts)
                    contact.ApplyImpulse();
            }


            foreach (Rigidbody rb in rigidbodies)
            {
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

            DispatchCollisionEvents();
        }
    }
}
