using System;
using System.Collections.Generic;
using static Automathon.Engine.Physics.Collision;

namespace Automathon.Engine.Physics
{
    public static class PhysicsManager
    {
        private static List<Rigidbody> rigidbodies = new();
        private static List<Contact> contacts = new();

        public static int Substeps = 6;
        /// <summary>Baumgarte factor: fraction of the penetration corrected each frame.</summary>
        public static Fixed KBias = 0.2;
        /// <summary>Penetration (in units) that is tolerated without position correction, avoids jitter.</summary>
        public static Fixed SlopPenetration = 0.03;

        public static void Initialize()
        {
            rigidbodies = new();
            contacts = new();
            Rigidbody.Added += OnRigidbodyAdded;
            Rigidbody.Removed += OnRigidbodyRemoved;
        }

        public static void Dispose()
        {
            Rigidbody.Added -= OnRigidbodyAdded;
            Rigidbody.Removed -= OnRigidbodyRemoved;
            rigidbodies.Clear();
            contacts.Clear();
        }

        private static void OnRigidbodyAdded(Rigidbody rigidbody)
            => rigidbodies.Add(rigidbody);

        private static void OnRigidbodyRemoved(Rigidbody rigidbody)
            => rigidbodies.Remove(rigidbody);

        private static void BroadPhase()
        {
            List<Contact> newContacts = new();

            void HandleBoxBoxContact(Rigidbody rigidbody1, BoxCollider boxCollider1, Rigidbody rigidbody2, BoxCollider boxCollider2)
            {
                Collision.BoxContact boxContact = Collision.BoxBoxClipping(boxCollider1, boxCollider2);

                if (!boxContact.Colliding)
                    return;

                Rigidbody referenceBody = boxContact.Reference == boxCollider1 ? rigidbody1 : rigidbody2;
                Rigidbody incidentBody = boxContact.Reference == boxCollider1 ? rigidbody2 : rigidbody1;

                void AddContact(FixedVector2 position)
                {
                    if (!boxContact.Reference.Contains(position))
                        return;

                    Contact contact = new Contact(referenceBody, incidentBody, boxContact.Reference, boxContact.Incident, position, boxContact.Normal, boxContact.Penetration);

                    newContacts.Add(contact);
                }

                AddContact(boxContact.ClippedIncidentFaceCoord1);
                AddContact(boxContact.ClippedIncidentFaceCoord2);
            }

            void HandleBoxCircleContact(Rigidbody boxRigidbody, BoxCollider boxCollider, Rigidbody circleRigidbody, CircleCollider circleCollider)
            {
                CollisionContact collisionContact = Collision.BoxCircleContact(boxCollider, circleCollider);

                if (!collisionContact.Colliding)
                    return;

                Rigidbody referenceBody = collisionContact.Reference == boxCollider ? boxRigidbody : circleRigidbody;
                Rigidbody incidentBody = collisionContact.Reference == boxCollider ? circleRigidbody : boxRigidbody;

                Contact contact = new Contact(referenceBody, incidentBody, collisionContact.Reference, collisionContact.Incident, collisionContact.Position, collisionContact.Normal, collisionContact.Penetration);

                newContacts.Add(contact);
            }

            void HandleCircleCircleContact(Rigidbody rigidbody1, CircleCollider circleCollider1, Rigidbody rigidbody2, CircleCollider circleCollider2)
            {
                if (!Collision.CircleCircle(circleCollider1, circleCollider2))
                    return;

                FixedVector2 delta = circleCollider2.WorldPosition - circleCollider1.WorldPosition;
                Fixed penetration = circleCollider1.Radius + circleCollider2.Radius - delta.Length();

                FixedVector2 normal = delta != FixedVector2.Zero ? delta.Normalized() : FixedVector2.Right;

                Contact contact = new Contact(rigidbody1, rigidbody2, circleCollider1, circleCollider2, circleCollider1.WorldPosition + delta / 2, normal, penetration);

                newContacts.Add(contact);
            }

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                for (int j = i + 1; j < rigidbodies.Count; j++)
                {
                    Rigidbody rigidbody = rigidbodies[i];
                    Rigidbody otherRigidbody = rigidbodies[j];

                    if (rigidbody.InvMass == Fixed.Zero && otherRigidbody.InvMass == Fixed.Zero)
                        continue;

                    void HandleColliders(Collider coll1, Collider coll2)
                    {
                        if (!LayerMatrix.GetCollision(coll1.Layer, coll2.Layer))
                            return;

                        if (coll1 is BoxCollider b1 && coll2 is BoxCollider b2)
                            HandleBoxBoxContact(rigidbody, b1, otherRigidbody, b2);
                        else if ((coll1 is BoxCollider b3 && coll2 is CircleCollider c1))
                            HandleBoxCircleContact(rigidbody, b3, otherRigidbody, c1);
                        else if (coll1 is CircleCollider c2 && coll2 is BoxCollider b4)
                            HandleBoxCircleContact(otherRigidbody, b4, rigidbody, c2);
                        else if (coll1 is CircleCollider c3 && coll2 is CircleCollider c4)
                            HandleCircleCircleContact(rigidbody, c3, otherRigidbody, c4);
                    }

                    if (rigidbody.Collider is CompositeCollider composite && otherRigidbody.Collider is CompositeCollider composite2)
                    {
                        foreach (Collider c in composite.Colliders)
                        {
                            foreach (Collider c2 in composite2.Colliders)
                                HandleColliders(c, c2);
                        }
                    }
                    else if (rigidbody.Collider is CompositeCollider composite3)
                    {
                        foreach (Collider c in composite3.Colliders)
                            HandleColliders(c, otherRigidbody.Collider);
                    }
                    else if (otherRigidbody.Collider is CompositeCollider composite4)
                    {
                        foreach (Collider c in composite4.Colliders)
                            HandleColliders(rigidbody.Collider, c);
                    }
                    else
                        HandleColliders(rigidbody.Collider, otherRigidbody.Collider);
                }
            }

            contacts = newContacts;
        }

        private static void ApplyForces()
        {
            foreach (Rigidbody rb in rigidbodies)
            {
                // Semi-implicit Euler: v += F / m * dt
                rb.Velocity += rb.Forces * rb.InvMass / GameplayConstants.FRAMERATE;
                rb.AngularVelocity += rb.Torque * rb.InvInertia / GameplayConstants.FRAMERATE;
            }
        }

        private static void DispatchCollisionEvents()
        {
            Dictionary<Collider, Dictionary<Collider, CollisionEvent>> colliderEvents = new();

            void AddToCollisionEvent(Collider self, Collider other, FixedVector2 position, FixedVector2 normal, Fixed penetration)
            {
                if (!colliderEvents.ContainsKey(self))
                    colliderEvents[self] = new Dictionary<Collider, CollisionEvent>();

                if (!colliderEvents[self].ContainsKey(other))
                    colliderEvents[self][other] = new CollisionEvent(self, other);

                colliderEvents[self][other].Contacts.Add(new CollisionEvent.ContactData(position, normal, penetration));
            }

            foreach (Contact contact in contacts)
            {
                if (!contact.IncidentCollider.IsTrigger)
                    AddToCollisionEvent(contact.ReferenceCollider, contact.IncidentCollider, contact.Position, contact.Normal, contact.Penetration);
                if (!contact.ReferenceCollider.IsTrigger)
                    AddToCollisionEvent(contact.IncidentCollider, contact.ReferenceCollider, contact.Position, -contact.Normal, contact.Penetration);
            }

            foreach (Dictionary<Collider, CollisionEvent> collisionEvents in colliderEvents.Values)
            {
                foreach (CollisionEvent collisionEvent in collisionEvents.Values)
                {
                    collisionEvent.AverageNormal = FixedVector2.Zero;

                    collisionEvent.MaxPenetration = Fixed.Zero;
                    foreach (CollisionEvent.ContactData contactData in collisionEvent.Contacts)
                    {
                        collisionEvent.AverageNormal += contactData.Normal;
                        collisionEvent.MaxPenetration = FixedMath.Max(collisionEvent.MaxPenetration, contactData.Penetration);
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
                rb.ParentEntity.Rotation += rb.AngularVelocity / GameplayConstants.FRAMERATE;

                //Putting it between -pi and +pi
                rb.ParentEntity.Rotation = FixedMath.WrapAngle(rb.ParentEntity.Rotation);

                rb.Forces = FixedVector2.Zero;
                rb.Torque = Fixed.Zero;
            }

            DispatchCollisionEvents();
        }
    }
}
