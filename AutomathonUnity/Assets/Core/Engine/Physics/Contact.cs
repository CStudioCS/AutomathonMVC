using System;

namespace Automathon.Engine.Physics
{
    /// <summary>
    /// A contact point between two rigidbodies, solved with sequential impulses (accumulated impulse clamping,
    /// Baumgarte position bias and Coulomb friction). Masses and accumulated impulses are stored as
    /// <see cref="FixedLong"/> so very light / very heavy bodies don't overflow or lose precision.
    /// </summary>
    public class Contact
    {
        public Rigidbody Reference { get; private set; }
        public Rigidbody Incident { get; private set; }

        public Collider ReferenceCollider { get; private set; }
        public Collider IncidentCollider { get; private set; }

        public FixedVector2 Position { get; private set; }
        /// <summary>Unit normal pointing from the reference towards the incident body.</summary>
        public FixedVector2 Normal { get; private set; }
        public Fixed Penetration { get; private set; }

        /// <summary>Accumulated normal and tangential impulses.</summary>
        public FixedLong Pn;
        public FixedLong Pt;

        private FixedLong normalMass;
        private FixedLong tangentialMass;
        private readonly Fixed friction;

        private FixedVector2 r1;
        private FixedVector2 r2;

        public Contact(Rigidbody reference, Rigidbody incident, Collider referenceCollider, Collider incidentCollider, FixedVector2 position, FixedVector2 normal, Fixed penetration)
        {
            Reference = reference;
            Incident = incident;
            ReferenceCollider = referenceCollider;
            IncidentCollider = incidentCollider;
            Position = position;
            Normal = normal;
            Penetration = penetration;
            friction = FixedMath.Sqrt(reference.Friction * incident.Friction);
        }

        public FixedVector2 ComputeR(Collider collider)
        {
            if (collider is BoxCollider boxCollider)
                return Position - boxCollider.WorldPosition;
            else if (collider is CircleCollider circleCollider)
                return Position - circleCollider.WorldPosition;
            else
                throw new Exception("Unsupported collider type");
        }

        /// <summary>
        /// Calculate effective masses
        /// </summary>
        public void PreStep()
        {
            r1 = ComputeR(ReferenceCollider);
            r2 = ComputeR(IncidentCollider);

            FixedLong invMassSum = (FixedLong)Reference.InvMass + Incident.InvMass;

            FixedLong EffectiveMass(FixedVector2 direction)
            {
                FixedLong rn1 = r1.CrossLong(direction);
                FixedLong rn2 = r2.CrossLong(direction);
                FixedLong k = invMassSum + Reference.InvInertia * rn1 * rn1 + Incident.InvInertia * rn2 * rn2;
                return k > FixedLong.Zero ? FixedLong.One / k : FixedLong.Zero;
            }

            normalMass = EffectiveMass(Normal);
            tangentialMass = EffectiveMass(Normal.OrthogonalCounterClockwise());
        }

        private static FixedVector2 PointVelocity(Rigidbody body, FixedVector2 r)
            => body.Velocity + r.OrthogonalCounterClockwise() * body.AngularVelocity; // v + w x r

        public void ApplyImpulse()
        {
            if (ReferenceCollider.IsTrigger || IncidentCollider.IsTrigger)
                return;

            // Relative velocity at the contact point
            FixedVector2 dV = PointVelocity(Incident, r2) - PointVelocity(Reference, r1);

            // Normal impulse
            FixedLong vn = dV.DotLong(Normal);
            Fixed penetrationToResolve = FixedMath.Max(Fixed.Zero, Penetration - PhysicsManager.SlopPenetration);
            FixedLong vbias = (FixedLong)PhysicsManager.KBias * penetrationToResolve * GameplayConstants.FRAMERATE;
            FixedLong dPn = normalMass * (vbias - vn);

            FixedLong previousPn = Pn;
            Pn = FixedMath.Max(previousPn + dPn, FixedLong.Zero);
            dPn = Pn - previousPn;

            // Friction impulse
            FixedVector2 tangent = Normal.OrthogonalCounterClockwise();
            FixedLong vt = dV.DotLong(tangent);
            FixedLong dPt = -(tangentialMass * vt);

            FixedLong maxPt = (FixedLong)friction * Pn;
            FixedLong previousPt = Pt;
            Pt = FixedMath.Clamp(previousPt + dPt, -maxPt, maxPt);
            dPt = Pt - previousPt;

            FixedVector2 P = Normal * dPn + tangent * dPt;

            Reference.Velocity -= P * Reference.InvMass;
            Reference.AngularVelocity -= Reference.InvInertia * r1.Cross(P);

            Incident.Velocity += P * Incident.InvMass;
            Incident.AngularVelocity += Incident.InvInertia * r2.Cross(P);
        }
    }
}
