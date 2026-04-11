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
            //Update coords using trigtable
        }
    }
}
