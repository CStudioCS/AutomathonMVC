using Automathon.Engine.Physics;
using System;

namespace Automathon.Utility
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
            width *= 2;
            height *= 2;
            cPosition *= 2;
            cRadius *= 2;

            Vector2Int halfSize = new Vector2Int(width / 2, height / 2);

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

        public class SATOutput
        {
            public bool IsCollision;
            public Vector2Int MinPenetrationAxis;
            public float Penetration;
            public int AxisIndex;
        }

        public static SATOutput SAT(Vector2Int[] polygon1, Vector2Int[] polygon2, Vector2Int[] axies)
        {
            for (int i = 0; i < axies.Length; i++)
                axies[i].Normalize();

            SATOutput result = new SATOutput();
            result.IsCollision = true;
            result.Penetration = float.PositiveInfinity;

            for (int i = 0; i < axies.Length; i++)
            {
                float min1 = float.PositiveInfinity;
                float max1 = float.NegativeInfinity;
                float min2 = float.PositiveInfinity;
                float max2 = float.NegativeInfinity;

                foreach (Vector2Int point in polygon1)
                {
                    float axisPos = Vector2Int.Dot(VectorHelper.Projection(point, axies[i]), axies[i]);
                    min1 = Math.Min(min1, axisPos);
                    max1 = Math.Max(max1, axisPos);
                }

                foreach (Vector2Int point in polygon2)
                {
                    float axisPos = Vector2Int.Dot(VectorHelper.Projection(point, axies[i]), axies[i]);
                    min2 = Math.Min(min2, axisPos);
                    max2 = Math.Max(max2, axisPos);
                }

                if (min2 >= max1 || max2 <= min1)
                {
                    result.IsCollision = false;
                    result.MinPenetrationAxis = Vector2Int.Zero;
                    result.Penetration = 0;
                    result.AxisIndex = -1;
                    return result;
                }
                else
                {
                    if (max1 >= min2 && max1 - min2 <= max2 - min1 && max1 - min2 < result.Penetration)
                    {
                        result.Penetration = max1 - min2;
                        result.MinPenetrationAxis = axies[i];
                        result.AxisIndex = i;
                    }
                    else if (max2 - min1 < result.Penetration)
                    {
                        result.Penetration = max2 - min1;
                        result.MinPenetrationAxis = axies[i];
                        result.AxisIndex = i;
                    }
                }
            }

            result.IsCollision = true;
            return result;
        }

        public static SATOutput BoxBoxSAT(Vector2Int[] box, Vector2Int[] box2)
        {
            if (box.Length != 4 || box2.Length != 4)
                throw new Exception("Rect vertices are not set properly, Rectangle has more or less than 4 vertices");

            //Indexes: UL = 0, UR = 1, LR = 2, LL = 3
            Vector2Int[] axies = new Vector2Int[4]
            {
                box[1] - box[0], //UR - UL
                box[1] - box[2], //UR - LR
                box2[1] - box2[0], //UL - UR
                box2[1] - box2[2], //UL - LL
            };

            return SAT(box, box2, axies);
        }
    }
}