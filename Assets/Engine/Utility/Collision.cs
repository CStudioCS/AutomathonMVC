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
            Vector2Int boxRight = box.Coords[1] - box.Coords[0];
            Vector2Int boxDown = box.Coords[3] - box.Coords[0];
            Vector2Int circlePos = circle.LocalPosition + circle.ParentEntity.Position;

            Vector2Int relativeCircPos = circlePos - box.Coords[0];
            Vector2Int rectCoordCirclePos = new Vector2Int(boxRight.X * relativeCircPos.X + boxDown.X * relativeCircPos.Y, boxRight.Y * relativeCircPos.X + boxDown.Y * relativeCircPos.Y);

            return AABBCircle(Vector2Int.Zero, 1, 1, rectCoordCirclePos, circle.Radius);
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
            public int PenetrationMilli { get; private set; }
            public int AxisIndex { get; private set; }

            public SATOutput(bool isCollision, Vector2Int minPenetrationAxis, int penetrationMilli, int axisIndex)
            {
                IsCollision = isCollision;
                MinPenetrationAxis = minPenetrationAxis;
                PenetrationMilli = penetrationMilli;
                AxisIndex = axisIndex;
            }
        }

        public static SATOutput SAT(Vector2Int[] polygon1, Vector2Int[] polygon2, Vector2Int[] axies)
        {
            for (int i = 0; i < axies.Length; i++)
                axies[i].NormalizeAtScale(1000); //normalized to a scale of 1000

            bool isCollision = true;
            Vector2Int minPenetrationAxis = Vector2Int.Zero;
            int penetrationMilli = int.MaxValue;
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
                    penetrationMilli = 0;
                    axisIndex = -1;
                    return new SATOutput(isCollision, minPenetrationAxis, penetrationMilli, axisIndex);
                }
                else
                {
                    if (max1 >= min2 && max1 - min2 <= max2 - min1 && max1 - min2 < penetrationMilli)
                    {
                        penetrationMilli = (int)((max1 - min2) / 1000);
                        minPenetrationAxis = axies[i];
                        axisIndex = i;
                    }
                    else if (max2 - min1 < penetrationMilli)
                    {
                        penetrationMilli = (int)((max2 - min1) / 1000);
                        minPenetrationAxis = axies[i];
                        axisIndex = i;
                    }
                }
            }

            isCollision = true;
            return new SATOutput(isCollision, minPenetrationAxis, penetrationMilli, axisIndex);
        }

        public static SATOutput BoxBoxSAT(BoxCollider box, BoxCollider box2)
        {
            //Indexes: UL = 0, UR = 1, LR = 2, LL = 3
            Vector2Int[] axies = new Vector2Int[4]
            {
                box.Coords[1] - box.Coords[0], //UR - UL
                box.Coords[1] - box.Coords[2], //UR - LR
                box2.Coords[1] - box2.Coords[0], //UL - UR
                box2.Coords[1] - box2.Coords[2], //UL - LL
            };

            return SAT(box.Coords, box2.Coords, axies);
        }

        public class BoxContact
        {
            public bool Colliding;

            public BoxCollider Reference;
            public BoxCollider Incident;
            public Vector2Int Normal;
            public float PenetrationMilli;

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
            contact.PenetrationMilli = sat.PenetrationMilli;

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
                    if (n.Dot(box.Coords[1] - box.Coords[0]) >= 0)
                        return (box.Coords[1], box.Coords[2]);

                    return (box.Coords[3], box.Coords[0]);
                }
                else
                {
                    if (n.Dot(box.Coords[0] - box.Coords[3]) >= 0)
                        return (box.Coords[0], box.Coords[1]);

                    return (box.Coords[2], box.Coords[3]);
                }
            }

            (contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2) = GetCollisionFace(reference, contact.Normal, sat.AxisIndex % 2 == 0);

            //ignore multiplication by height and width, just there for scaling
            bool incidentXAxis = Math.Abs(contact.Normal.Dot((incident.Coords[1] - incident.Coords[0]) * incident.Height)) >= Math.Abs(contact.Normal.Dot((incident.Coords[2] - incident.Coords[1]) * incident.Width));
            (Vector2Int inc1, Vector2Int inc2) = GetCollisionFace(incident, -contact.Normal, incidentXAxis); //incident face

            //Clip the incident face to the reference face
            contact.ClippedIncidentFaceCoord1 = inc1.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);
            contact.ClippedIncidentFaceCoord2 = inc2.ClipBetween(contact.ReferenceFaceCoord1, contact.ReferenceFaceCoord2);

            return contact;
        }
    }
}