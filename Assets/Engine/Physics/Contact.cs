using Automathon.Engine.Physics;
using Automathon.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine.Physics
{
    public class Contact
    {
        public Rigidbody Reference;
        public Rigidbody Incident;

        private Vector2Int position;
        private Vector2Int normal;
        private int penetration;

        public int Pn; //accumulated impulses
        public int Pt;

        private int normalMass;
        private int tangentialMass;
        private int friction;

        private Vector2Int r1;
        private Vector2Int r2;

        public Contact(Rigidbody reference, Rigidbody incident, Vector2Int position, Vector2Int normal, int penetration)
        {
            Reference = reference;
            Incident = incident;
            this.position = position;
            this.normal = normal;
            this.penetration = penetration;
            friction = IntMath.Isqrt(reference.Friction * incident.Friction);

            r1 = ComputeR(reference.Collider);
            r2 = ComputeR(incident.Collider);
        }

        public Vector2Int ComputeR(Collider collider)
        {
            if (collider is BoxCollider boxCollider)
                return position - boxCollider.WorldPosition;
            else if (collider is CircleCollider circleCollider)
                return position - circleCollider.WorldPosition;
            else
                throw new Exception("Unsupported collider type");
        }

        /// <summary>
        /// Calculate Effective masses
        /// </summary>
        public void PreStep()
        {
            Vector2Int DoubleVectProd(Vector2Int r, Vector2Int n)
                => new Vector2Int(-r.Y * (r.X * n.Y - r.Y * n.X), r.X * (r.X * n.Y - r.Y * n.X));

            Vector2Int v = Reference.InvIMilli * DoubleVectProd(r1, normal) + Incident.InvIMilli * DoubleVectProd(r2, normal);
            normalMass = 1 / (Reference.InvMassMilli + Incident.InvMassMilli + v.Dot(normal));

            Vector2Int tangent = normal.OrthogonalCounterClockwise();
            tangentialMass = 1 / (Reference.InvMassMilli + Incident.InvMassMilli + v.Dot(tangent));
        }

        public void ApplyImpulse()
        {
            Vector2Int dV = Incident.Velocity + Incident.AngularVelocityMilli * new Vector2Int(-r2.Y, r2.X) - (Reference.Velocity + Reference.AngularVelocityMilli * new Vector2Int(-r1.Y, r1.X)); //Vector Product translated in coords because not 3D lol

            int vn = Vector2Int.Dot(dV, normal);
            int vbias = KBias * Math.Max(0, penetration - SlopPenetration) / GameplayConstants.FRAMERATE;
            int pnt = Math.Max((-vn + vbias) * normalMass, 0);
            int tpp = pnt;

            float temp = Pn; //Accumulated impulse
            Pn = Math.Max(Pn + pnt, 0);
            pnt = Pn - temp;

            if (Math.Abs(tpp - pnt) >= 0.01f)
                Debug.Log(pnt - tpp);

            Vector2Int tangent = normal.OrthogonalCounterClockwise();
            int vt = dV.Dot(tangent);

            int ptt = Math.Clamp(-vt * tangentialMass, -friction * Pn, friction * Pn);
            temp = Pt;
            Pt = Math.Clamp(Pt + ptt, -friction * Pn, friction * Pn);
            ptt = Pt - temp;

            Vector2Int P = pnt * normal + ptt * tangent;
            Reference.Velocity -= P * Reference.InvMass;
            Reference.AngularVelocityMilli -= Reference.InvI * (r1.X * P.Y - r1.Y * P.X);
            Incident.Velocity += P * Incident.InvMass;
            Incident.AngularVelocityMilli += Incident.InvI * (r2.X * P.Y - r2.Y * P.X);

            //TODO: Angular velocity
            //TODO: Add bias impulse
        }
    }
}
