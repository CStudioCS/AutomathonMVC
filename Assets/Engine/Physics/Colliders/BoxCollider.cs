using Automathon.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public Vector2Int WorldPosition => ParentEntity.Position + LocalCenterPosition;
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
            if (collider is BoxCollider b)
            {
                Debug.Log(Collision.BoxBoxSAT(b, this).IsCollision);
                return Collision.BoxBoxSAT(b, this).IsCollision;
            }
            else if (collider is CircleCollider c)
                return Collision.BoxCircle(this, c);
            else
                throw new NotImplementedException();
        }

        public override void PhysicsUpdate()
        {
            Vector2Int right = new Vector2Int(TrigTable.Cos(RotationMillirad), TrigTable.Sin(RotationMillirad)); //cos multiplied by 1000
            Vector2Int up = right.Normal();
            int halfWidth = Width / 2; //yes if the width is non divisible by 2 this has issues but gnagnagna we gotta use ints
            int halfHeight = Height / 2;

            Coords = new Vector2Int[4]
            {
                WorldPosition + (-right * halfWidth + up * halfHeight) / 1000,
                WorldPosition + (right * halfWidth + up * halfHeight) / 1000,
                WorldPosition + (right * halfWidth - up * halfHeight) / 1000,
                WorldPosition + (-right * halfWidth - up * halfHeight) / 1000,
            };
        }
    }
}
