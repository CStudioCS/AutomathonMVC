using Automathon.Engine.Physics;
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
        private float penetration;

        public float Pn; //accumulated impulses
        public float Pt;

        private float normalMass;
        private float tangentialMass;
        private float friction;

        private Vector2Int r1;
        private Vector2Int r2;

        public Contact(Rigidbody reference, Rigidbody incident, Vector2Int position, Vector2Int normal, float penetration)
        {
            Reference = reference;
            Incident = incident;
            this.position = position;
            this.normal = normal;
            this.penetration = penetration;
            friction = (float)Math.Sqrt(reference.Friction * incident.Friction);

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
    }
}
