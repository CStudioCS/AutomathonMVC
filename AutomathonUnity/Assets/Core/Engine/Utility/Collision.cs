using System;

namespace Automathon.Engine.Physics
{
    public static class Collision
    {
        public static bool BoxCircle(BoxCollider box, CircleCollider circle)
        {
            // Express the circle center in the box's local frame, then do an AABB-circle test
            FixedVector2 right = (box.WorldVertices[1] - box.WorldVertices[0]).Normalized();
            FixedVector2 up = (box.WorldVertices[1] - box.WorldVertices[2]).Normalized();
            FixedVector2 relative = circle.WorldPosition - box.WorldPosition;

            Fixed halfWidth = box.Width / 2;
            Fixed halfHeight = box.Height / 2;
            FixedVector2 local = new FixedVector2(relative.Dot(right), relative.Dot(up));
            FixedVector2 closest = new FixedVector2(
                FixedMath.Clamp(local.X, -halfWidth, halfWidth),
                FixedMath.Clamp(local.Y, -halfHeight, halfHeight));

            return (local - closest).LengthSquaredLong() < FixedLong.Multiply(circle.Radius, circle.Radius);
        }

        public static bool CircleCircle(CircleCollider circle1, CircleCollider circle2)
        {
            Fixed radiusSum = circle1.Radius + circle2.Radius;
            return (circle2.WorldPosition - circle1.WorldPosition).LengthSquaredLong() < FixedLong.Multiply(radiusSum, radiusSum);
        }

        public class SATOutput
        {
            public bool IsCollision { get; private set; }
            /// <summary>Unit axis of minimum penetration.</summary>
            public FixedVector2 MinPenetrationAxis { get; private set; }
            public Fixed Penetration { get; private set; }
            public int AxisIndex { get; private set; }

            public SATOutput(bool isCollision, FixedVector2 minPenetrationAxis, Fixed penetration, int axisIndex)
            {
                IsCollision = isCollision;
                MinPenetrationAxis = minPenetrationAxis;
                Penetration = penetration;
                AxisIndex = axisIndex;
            }
        }

        public static SATOutput SAT(FixedVector2[] polygon1, FixedVector2[] polygon2, FixedVector2[] axes)
        {
            for (int i = 0; i < axes.Length; i++)
                axes[i] = axes[i].Normalized();

            FixedVector2 minPenetrationAxis = FixedVector2.Zero;
            FixedLong penetration = FixedLong.MaxValue;
            int axisIndex = -1;

            for (int i = 0; i < axes.Length; i++)
            {
                FixedLong min1 = FixedLong.MaxValue;
                FixedLong max1 = FixedLong.MinValue;
                FixedLong min2 = FixedLong.MaxValue;
                FixedLong max2 = FixedLong.MinValue;

                foreach (FixedVector2 point in polygon1)
                {
                    FixedLong axisPos = point.DotLong(axes[i]);
                    min1 = FixedMath.Min(min1, axisPos);
                    max1 = FixedMath.Max(max1, axisPos);
                }

                foreach (FixedVector2 point in polygon2)
                {
                    FixedLong axisPos = point.DotLong(axes[i]);
                    min2 = FixedMath.Min(min2, axisPos);
                    max2 = FixedMath.Max(max2, axisPos);
                }

                if (min2 >= max1 || max2 <= min1)
                    return new SATOutput(false, FixedVector2.Zero, Fixed.Zero, -1);

                if (max1 - min2 <= max2 - min1)
                {
                    if (max1 - min2 < penetration)
                    {
                        penetration = max1 - min2;
                        minPenetrationAxis = axes[i];
                        axisIndex = i;
                    }
                }
                else if (max2 - min1 < penetration)
                {
                    penetration = max2 - min1;
                    minPenetrationAxis = axes[i];
                    axisIndex = i;
                }
            }

            return new SATOutput(true, minPenetrationAxis, (Fixed)penetration, axisIndex);
        }

        public static SATOutput BoxBoxSAT(BoxCollider box, BoxCollider box2)
        {
            //Indexes: UL = 0, UR = 1, LR = 2, LL = 3
            FixedVector2[] axes = new FixedVector2[4]
            {
                box.WorldVertices[1] - box.WorldVertices[0], //UR - UL
                box.WorldVertices[1] - box.WorldVertices[2], //UR - LR
                box2.WorldVertices[1] - box2.WorldVertices[0],
                box2.WorldVertices[1] - box2.WorldVertices[2],
            };

            return SAT(box.WorldVertices, box2.WorldVertices, axes);
        }

        public class CollisionContact
        {
            public bool Colliding;

            public Collider Reference;
            public Collider Incident;
            public FixedVector2 Position;
            /// <summary>Unit normal, pointing from the reference towards the incident collider.</summary>
            public FixedVector2 Normal;
            public Fixed Penetration;
        }

        public class BoxContact : CollisionContact
        {
            public FixedVector2 ReferenceFaceCoord1;
            public FixedVector2 ReferenceFaceCoord2;
            public FixedVector2 ClippedIncidentFaceCoord1;
            public FixedVector2 ClippedIncidentFaceCoord2;
        }

        public static BoxContact BoxBoxClipping(BoxCollider b1, BoxCollider b2)
        {
            SATOutput sat = BoxBoxSAT(b1, b2);

            BoxContact contact = new BoxContact();

            if (sat.AxisIndex == -1)
            {
                contact.Colliding = false;
                return contact;
            }

            if (sat.AxisIndex >= 4)
                throw new Exception("sat axis index is not within expected bounds");

            BoxCollider reference = sat.AxisIndex <= 1 ? b1 : b2;
            BoxCollider incident = sat.AxisIndex <= 1 ? b2 : b1;

            contact.Colliding = true;
            contact.Reference = reference;
            contact.Incident = incident;
            contact.Penetration = sat.Penetration;

            FixedVector2 refToInc = incident.WorldPosition - reference.WorldPosition;

            //Switch normal direction to be ref -> inc
            contact.Normal = refToInc.DotLong(sat.MinPenetrationAxis) >= FixedLong.Zero ? sat.MinPenetrationAxis : -sat.MinPenetrationAxis;

            (FixedVector2, FixedVector2) GetCollisionFace(BoxCollider box, FixedVector2 n, bool isXAxis)
            {
                if (isXAxis)
                {
                    //Contact normal is more parallel to the face along the x axis, so the face is along the y axis
                    //Check which of the two faces corresponding to the axis is the one facing the other box
                    if (n.DotLong(box.WorldVertices[1] - box.WorldVertices[0]) >= FixedLong.Zero)
                        return (box.WorldVertices[1], box.WorldVertices[2]);

                    return (box.WorldVertices[3], box.WorldVertices[0]);
                }
                else
                {
                    if (n.DotLong(box.WorldVertices[0] - box.WorldVertices[3]) >= FixedLong.Zero)
                        return (box.WorldVertices[0], box.WorldVertices[1]);

                    return (box.WorldVertices[2], box.WorldVertices[3]);
                }
            }

            (contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2) = GetCollisionFace(reference, contact.Normal, sat.AxisIndex % 2 == 0);

            // Which incident face is the most parallel to the normal (compare with unit edge directions)
            FixedVector2 incidentRight = (incident.WorldVertices[1] - incident.WorldVertices[0]).Normalized();
            FixedVector2 incidentDown = (incident.WorldVertices[2] - incident.WorldVertices[1]).Normalized();
            bool incidentXAxis = contact.Normal.DotLong(incidentRight).Abs() >= contact.Normal.DotLong(incidentDown).Abs();
            (FixedVector2 inc1, FixedVector2 inc2) = GetCollisionFace(incident, -contact.Normal, incidentXAxis); //incident face

            //Clip the incident face to the reference face
            contact.ClippedIncidentFaceCoord1 = inc1.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);
            contact.ClippedIncidentFaceCoord2 = inc2.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);

            return contact;
        }

        public static CollisionContact BoxCircleContact(BoxCollider box, CircleCollider circle)
        {
            CollisionContact collisionContact = new CollisionContact();

            // Box right and up unit vectors
            FixedVector2 right = (box.WorldVertices[1] - box.WorldVertices[0]).Normalized();
            FixedVector2 up = (box.WorldVertices[1] - box.WorldVertices[2]).Normalized();

            Fixed halfWidth = box.Width / 2;
            Fixed halfHeight = box.Height / 2;

            // Transform circle center into box local space
            FixedVector2 relativeCirclePos = circle.WorldPosition - box.WorldPosition;
            FixedVector2 localCircleCenter = new FixedVector2(relativeCirclePos.Dot(right), relativeCirclePos.Dot(up));

            // Find closest point on box by clamping to half-extents
            FixedVector2 closest = new FixedVector2(
                FixedMath.Clamp(localCircleCenter.X, -halfWidth, halfWidth),
                FixedMath.Clamp(localCircleCenter.Y, -halfHeight, halfHeight));

            // If circle center is inside the box, snap closest to the nearest face
            bool inside = localCircleCenter == closest;
            if (inside)
            {
                // |x| / hw > |y| / hh  <=>  |x| * hh > |y| * hw
                bool nearerToX = FixedLong.Multiply(localCircleCenter.X.Abs(), halfHeight) > FixedLong.Multiply(localCircleCenter.Y.Abs(), halfWidth);
                if (nearerToX)
                    closest.X = localCircleCenter.X > 0 ? halfWidth : -halfWidth;
                else
                    closest.Y = localCircleCenter.Y > 0 ? halfHeight : -halfHeight;
            }

            FixedVector2 delta = localCircleCenter - closest;
            Fixed dist = delta.Length();

            // Early out: circle is outside and not touching
            if (!inside && dist > circle.Radius)
            {
                collisionContact.Colliding = false;
                return collisionContact;
            }

            // Normal in local space, pointing from box surface toward circle center
            FixedVector2 localNormal = delta != FixedVector2.Zero ? delta.Normalized() : FixedVector2.Up;
            if (inside) localNormal = -localNormal;

            // Rotate normal back to world space
            FixedVector2 normal = (right * localNormal.X + up * localNormal.Y).Normalized();

            Fixed separation = inside ? -(circle.Radius + dist) : dist - circle.Radius;

            Fixed contactOffsetSign = inside ? 1 : -1;
            FixedVector2 contactPosition = circle.WorldPosition + normal * (circle.Radius * contactOffsetSign);

            collisionContact.Colliding = true;
            collisionContact.Normal = normal;
            collisionContact.Reference = box;
            collisionContact.Incident = circle;
            collisionContact.Penetration = -separation;
            collisionContact.Position = contactPosition;

            return collisionContact;
        }
    }
}
