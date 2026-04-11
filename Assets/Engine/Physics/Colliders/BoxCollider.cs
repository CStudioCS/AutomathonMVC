using Automathon.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public Vector2Int LocalCenterPosition;
        public int Width;
        public int Height;
        public int RotationMillirad;

        public Vector2Int[] Coords;

        public BoxCollider(Vector2Int localPosition, int width, int height, int rotationMillirad)
        {
            LocalCenterPosition = localPosition;
            Width = width;
            Height = height;
            RotationMillirad = rotationMillirad;
        }

        public override bool Colliding(Collider collider)
        {
            throw new NotImplementedException();
        }

        public override void PhysicsUpdate()
        {
            Vector2Int right = new Vector2Int(TrigTable.Cos(RotationMillirad), TrigTable.Sin(RotationMillirad));
            Vector2Int up = right.Normal();
            int halfWidth = Width / 2; //yes if the width is non divisible by 2 this has issues but gnagnagna we gotta use ints
            int halfHeight = Height / 2;

            Coords = new Vector2Int[4]
            {
                LocalCenterPosition - right * halfWidth + up * halfHeight,
                LocalCenterPosition + right * halfWidth + up * halfHeight,
                LocalCenterPosition + right * halfWidth - up * halfHeight,
                LocalCenterPosition - right * halfWidth - up * halfHeight,
            };
        }
    }
}
