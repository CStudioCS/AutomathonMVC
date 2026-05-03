using System;

namespace Automathon.Engine.Physics
{
    public static class Collision
    {
        //There's gonna be a lot of approximations here, and the game would probably run better
        //if they FUIBZEIFUBEZFOUB let me use floats

        public static bool BoxCircle(BoxCollider box, CircleCollider circle)
        {
            //Calculate the circle coordinates in the boxes relative coordinate system, then simply use AABB-Circle collision with the box at position (0, 0)
            //We basically rotate the circle instead of the rectangle
            Vector2Int boxRight = box.WorldVertices[1] - box.WorldVertices[0];
            Vector2Int boxDown = box.WorldVertices[3] - box.WorldVertices[0];

            Vector2Int relativeCirclePos = circle.WorldPosition - box.WorldVertices[0];
            Vector2Int rectCoordCirclePos = new Vector2Int(relativeCirclePos.Dot(boxRight) / box.Width, relativeCirclePos.Dot(boxDown) / box.Height);

            return AABBCircle(Vector2Int.Zero, box.Width, box.Height, rectCoordCirclePos, circle.Radius);
        }

        public static bool AABBCircle(Vector2Int rectTopLeftPos, int width, int height, Vector2Int cPosition, int cRadius)
        {
            //work in doubled coord space else it don't work
            rectTopLeftPos *= 2;
            cPosition *= 2;
            cRadius *= 2;
            Vector2Int halfSize = new Vector2Int(width, height);


            int distX = Math.Abs(cPosition.X - (rectTopLeftPos.X + halfSize.X));
            int distY = Math.Abs(cPosition.Y - (rectTopLeftPos.Y - halfSize.Y));

            if (distX >= cRadius + halfSize.X || distY >= cRadius + halfSize.Y)
                return false;
            if (distX < halfSize.X && distY < halfSize.Y)
                return true;

            distX -= halfSize.X;
            distY -= halfSize.Y;

            if (distX <= halfSize.X || distY <= halfSize.Y)
                return true;

            if (distX * distX + distY * distY < cRadius * cRadius)
                return true;

            return false;
        }

        public static bool CircleCircle(CircleCollider circle1, CircleCollider circle2)
            => (circle2.WorldPosition - circle1.WorldPosition).LengthSquared() < (circle1.Radius + circle2.Radius) * (circle1.Radius + circle2.Radius);

        public class SATOutput
        {
            public bool IsCollision { get; private set; }
            public Vector2Int MinPenetrationAxis { get; private set; }
            public int Penetration { get; private set; }
            public int AxisIndex { get; private set; }

            public SATOutput(bool isCollision, Vector2Int minPenetrationAxis, int penetrationMilli, int axisIndex)
            {
                IsCollision = isCollision;
                MinPenetrationAxis = minPenetrationAxis;
                Penetration = penetrationMilli;
                AxisIndex = axisIndex;
            }
        }

        public static SATOutput SAT(Vector2Int[] polygon1, Vector2Int[] polygon2, Vector2Int[] axies)
        {
            for (int i = 0; i < axies.Length; i++)
                axies[i].NormalizeAtScale(1000); //normalized to a scale of 1000

            bool isCollision = true;
            Vector2Int minPenetrationAxis = Vector2Int.Zero;
            long penetration = int.MaxValue;
            int axisIndex = -1;

            for (int i = 0; i < axies.Length; i++)
            {
                long min1 = long.MaxValue;
                long max1 = long.MinValue;
                long min2 = long.MaxValue;
                long max2 = long.MinValue;

                foreach (Vector2Int point in polygon1)
                {
                    long axisPos = point.Dot(axies[i]);
                    min1 = Math.Min(min1, axisPos);
                    max1 = Math.Max(max1, axisPos);
                }

                foreach (Vector2Int point in polygon2)
                {
                    long axisPos = point.Dot(axies[i]);
                    min2 = Math.Min(min2, axisPos);
                    max2 = Math.Max(max2, axisPos);
                }

                if (min2 >= max1 || max2 <= min1)
                {
                    isCollision = false;
                    minPenetrationAxis = Vector2Int.Zero;
                    penetration = 0;
                    axisIndex = -1;
                    return new SATOutput(isCollision, minPenetrationAxis, (int)penetration, axisIndex);
                }
                else
                {
                    if (max1 >= min2 && max1 - min2 <= max2 - min1 && max1 - min2 < penetration)
                    {
                        penetration = (int)(max1 - min2);
                        minPenetrationAxis = axies[i];
                        axisIndex = i;
                    }
                    else if (max2 - min1 < penetration)
                    {
                        penetration = (int)(max2 - min1);
                        minPenetrationAxis = axies[i];
                        axisIndex = i;
                    }
                }
            }

            isCollision = true;
            //The true penetration is penetration / 1000 since the axis is normalized at a scale by 1000
            return new SATOutput(isCollision, minPenetrationAxis, (int)(penetration / 1000), axisIndex);
        }

        public static SATOutput BoxBoxSAT(BoxCollider box, BoxCollider box2)
        {
            //Indexes: UL = 0, UR = 1, LR = 2, LL = 3
            Vector2Int[] axies = new Vector2Int[4]
            {
                box.WorldVertices[1] - box.WorldVertices[0], //UR - UL
                box.WorldVertices[1] - box.WorldVertices[2], //UR - LR
                box2.WorldVertices[1] - box2.WorldVertices[0], //UL - UR
                box2.WorldVertices[1] - box2.WorldVertices[2], //UL - LL
            };

            return SAT(box.WorldVertices, box2.WorldVertices, axies);
        }

        public class CollisionContact
        {
            public bool Colliding;

            public Collider Reference;
            public Collider Incident;
            public Vector2Int Position;
            public Vector2Int Normal;
            public int Penetration;
        }

        public class BoxContact : CollisionContact
        {
            public Vector2Int ReferenceFaceCoord1;
            public Vector2Int ReferenceFaceCoord2;
            public Vector2Int ClippedIncidentFaceCoord1;
            public Vector2Int ClippedIncidentFaceCoord2;
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

            BoxCollider reference, incident;
            reference = sat.AxisIndex <= 1 ? b1 : b2;
            incident = sat.AxisIndex <= 1 ? b2 : b1;


            contact.Colliding = true;
            contact.Reference = reference;
            contact.Incident = incident;
            contact.Penetration = sat.Penetration;

            Vector2Int refToInc = incident.WorldPosition - reference.WorldPosition;

            //Switch normal direction to be ref -> inc
            if (refToInc.Dot(sat.MinPenetrationAxis) >= 0)
                contact.Normal = sat.MinPenetrationAxis;
            else
                contact.Normal = -sat.MinPenetrationAxis;

            (Vector2Int, Vector2Int) GetCollisionFace(BoxCollider box, Vector2Int n, bool isXAxis)
            {
                if (isXAxis)
                {
                    //Contact normal is more parallel to the face along the x axis, so the face is along the y axis

                    //Check between which of the faces corresponding to the axis is actually the one being collided with
                    //(is the one facing the other box)
                    if (n.Dot(box.WorldVertices[1] - box.WorldVertices[0]) >= 0)
                        return (box.WorldVertices[1], box.WorldVertices[2]);

                    return (box.WorldVertices[3], box.WorldVertices[0]);
                }
                else
                {
                    if (n.Dot(box.WorldVertices[0] - box.WorldVertices[3]) >= 0)
                        return (box.WorldVertices[0], box.WorldVertices[1]);

                    return (box.WorldVertices[2], box.WorldVertices[3]);
                }
            }

            (contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2) = GetCollisionFace(reference, contact.Normal, sat.AxisIndex % 2 == 0);

            //ignore multiplication by height and width, just there for scaling
            bool incidentXAxis = Math.Abs(contact.Normal.Dot((incident.WorldVertices[1] - incident.WorldVertices[0]) * incident.Height)) >= Math.Abs(contact.Normal.Dot((incident.WorldVertices[2] - incident.WorldVertices[1]) * incident.Width));
            (Vector2Int inc1, Vector2Int inc2) = GetCollisionFace(incident, -contact.Normal, incidentXAxis); //incident face

            //Clip the incident face to the reference face
            contact.ClippedIncidentFaceCoord1 = inc1.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);
            contact.ClippedIncidentFaceCoord2 = inc2.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);

            return contact;
        }

        public static CollisionContact BoxCircleContact(BoxCollider box, CircleCollider circle)
        {
            CollisionContact collisionContact = new CollisionContact();

            // Calculate Box right and up direction vectors manually using RotationMillirad at milli scale (1000)
            Vector2Int rightMilli = box.WorldVertices[1] - box.WorldVertices[0];
            Vector2Int upMilli = box.WorldVertices[1] - box.WorldVertices[2];
            rightMilli.NormalizeAtScale(1000);
            upMilli.NormalizeAtScale(1000);

            int halfWidth = box.Width / 2;
            int halfHeight = box.Height / 2;

            // Transform circle center into box local space
            Vector2Int relativeCirclePos = circle.WorldPosition - box.WorldPosition;

            Vector2Int localCenter = new Vector2Int(
                relativeCirclePos.Dot(rightMilli) / 1000,
                relativeCirclePos.Dot(upMilli) / 1000
            );

            // Find closest point on box by clamping to half-extents
            Vector2Int closest = new Vector2Int(
                Math.Clamp(localCenter.X, -halfWidth, halfWidth),
                Math.Clamp(localCenter.Y, -halfHeight, halfHeight)
            );

            // If circle center is inside the box, snap closest to the nearest face
            bool inside = localCenter == closest;
            if (inside)
            {
                // To compare x / hw and y / hh correctly with integers, cross multiply:
                // |x| * hh > |y| * hw
                long absX_hh = Math.Abs(localCenter.X) * halfHeight;
                long absY_hw = Math.Abs(localCenter.Y) * halfWidth;

                bool nearerToX = absX_hh > absY_hw;
                if (nearerToX)
                    closest.X = localCenter.X > 0 ? halfWidth : -halfWidth;
                else
                    closest.Y = localCenter.Y > 0 ? halfHeight : -halfHeight;
            }

            Vector2Int delta = localCenter - closest;
            int dist = delta.Length(); //reduce truncation errors

            // Early out: circle is outside and not touching
            if (!inside && dist > circle.Radius)
            {
                collisionContact.Colliding = false;
                return collisionContact;
            }

            // Normal in local space, pointing from box surface toward circle center
            // Scaled by 1000 to maintain milli format
            Vector2Int localNormalMilli = dist > 0 ? (delta * 1000) / dist : new Vector2Int(0, 1000);
            if (inside) localNormalMilli = -localNormalMilli;

            // Rotate normal back to world space. 
            // Invert the local to world logic from standard matrix rotation.
            // upMilli here replaces the conventional .Column2 whereas right replaces .Column1
            Vector2Int normalMilli = new Vector2Int(
                (int)(((long)rightMilli.X * localNormalMilli.X + (long)upMilli.X * localNormalMilli.Y) / 1000),
                (int)(((long)rightMilli.Y * localNormalMilli.X + (long)upMilli.Y * localNormalMilli.Y) / 1000)
            );

            normalMilli.NormalizeAtScale(1000);

            int separation = inside ? -(circle.Radius + dist) : dist - circle.Radius;

            Vector2Int contactPosition = new Vector2Int(
                circle.WorldPosition.X - (int)(((long)normalMilli.X * circle.Radius) / 1000),
                circle.WorldPosition.Y - (int)(((long)normalMilli.Y * circle.Radius) / 1000)
            );


            collisionContact.Colliding = true;
            collisionContact.Normal = normalMilli;
            collisionContact.Reference = box;
            collisionContact.Incident = circle;
            collisionContact.Penetration = -separation;
            collisionContact.Position = contactPosition;

            return collisionContact;
        }
    }
}