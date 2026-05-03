using Automathon.Engine.Utility;
using System;

namespace Automathon.Engine.Physics
{
    public class Contact
    {
        public Rigidbody Reference { get; private set; }
        public Rigidbody Incident { get; private set; }

        public Collider ReferenceCollider { get; private set; }
        public Collider IncidentCollider { get; private set; }

        public Vector2Int Position { get; private set; }
        public Vector2Int NormalMilli { get; private set; } //normal vector scaled by 1000
        public int Penetration { get; private set; }

        public int Pn; //accumulated impulses
        public int Pt;

        private int normalMassMilli;
        private int tangentialMassMilli;
        private int frictionMilli;

        private Vector2Int r1;
        private Vector2Int r2;

        public Contact(Rigidbody reference, Rigidbody incident, Collider referenceCollider, Collider incidentCollider, Vector2Int position, Vector2Int normalMilli, int penetration)
        {
            Reference = reference;
            Incident = incident;
            ReferenceCollider = referenceCollider;
            IncidentCollider = incidentCollider;
            Position = position;
            NormalMilli = normalMilli;
            Penetration = penetration;
            frictionMilli = IntMath.Isqrt(reference.FrictionMilli * incident.FrictionMilli) / 1000;
        }

        public Vector2Int ComputeR(Collider collider)
        {
            if (collider is BoxCollider boxCollider)
                return Position - boxCollider.WorldPosition;
            else if (collider is CircleCollider circleCollider)
                return Position - circleCollider.WorldPosition;
            else
                throw new Exception("Unsupported collider type");
        }

        /// <summary>
        /// Calculate Effective masses
        /// </summary>
        public void PreStep()
        {
            r1 = ComputeR(Reference.Collider);
            r2 = ComputeR(Incident.Collider);

            Vector2Int DoubleVectProd(Vector2Int r, Vector2Int n)
                => new Vector2Int(-r.Y * (r.X * n.Y - r.Y * n.X), r.X * (r.X * n.Y - r.Y * n.X));

            //one division by 1000 because the normal is mult by 1000, and another one because of InvIMilli
            Vector2Int v = (Reference.InvIMicro * DoubleVectProd(r1, NormalMilli) + Incident.InvIMicro * DoubleVectProd(r2, NormalMilli)) / 1000000000 / 1000000;
            normalMassMilli = 1000 / ((Reference.InvMassMilli + Incident.InvMassMilli) / 1000 + v.Dot(NormalMilli));

            Vector2Int tangent = NormalMilli.OrthogonalCounterClockwise();
            tangentialMassMilli = 1000 / ((Reference.InvMassMilli + Incident.InvMassMilli) / 1000 + v.Dot(tangent));
        }

        public void ApplyImpulse()
        {
            //You will fucking cry reading this
            //The normalMilli vector is scaled by 1000
            //InvMassMilli and InvIMilli are also scaled by 1000
            //normalMassMilli and tangentialMassMilli are also scaled by 1000
            //oh yeah also frictionMilli
            //Therefore there is a bunch of /1000 everywhere in the code

            Vector2Int dV = Incident.Velocity + Incident.AngularVelocityMilli * new Vector2Int(-r2.Y, r2.X) / 1000 - (Reference.Velocity + Reference.AngularVelocityMilli * new Vector2Int(-r1.Y, r1.X) / 1000); //Vector Product translated in coords because not 3D lol

            int vn = dV.Dot(NormalMilli) / 1000;
            int vbias = PhysicsManager.KBiasMilli * Math.Max(0, Penetration - PhysicsManager.SlopPenetration) * GameplayConstants.FRAMERATE / 1000;
            int pnt = Math.Max((-vn + vbias) * normalMassMilli / 1000, 0);
            int tpp = pnt;

            int temp = Pn; //Accumulated impulse
            Pn = Math.Max(Pn + pnt, 0);
            pnt = Pn - temp;

            /*if (Math.Abs(tpp - pnt) >= 0.01f)   
                Debug.Log(pnt - tpp);*/

            Vector2Int tangent = NormalMilli.OrthogonalCounterClockwise();
            int vt = dV.Dot(tangent) / 1000;

            int ptt = (int)Math.Clamp(-vt * tangentialMassMilli / 1000, -frictionMilli * Pn / 1000, frictionMilli * Pn / 1000);
            temp = Pt;
            Pt = Math.Clamp(Pt + ptt, -frictionMilli * Pn, frictionMilli * Pn);
            ptt = Pt - temp;

            Vector2Int P = pnt * NormalMilli / 1000 + ptt * tangent / 1000;

            Reference.Velocity -= P * Reference.InvMassMilli / 1000;
            Reference.AngularVelocityMilli -= Reference.InvIMicro * (r1.X * P.Y - r1.Y * P.X) / 1000000;
            Incident.Velocity += P * Incident.InvMassMilli / 1000;
            Incident.AngularVelocityMilli += Incident.InvIMicro * (r2.X * P.Y - r2.Y * P.X) / 1000000;
        }
    }
}
