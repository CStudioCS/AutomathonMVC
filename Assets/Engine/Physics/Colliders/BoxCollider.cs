using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public Vector2Int LocalPosition;
        public int Width;
        public int Height;
        public int RotationMillirad;

        public BoxCollider(Vector2Int localPosition, int width, int height, int rotationMillirad)
        {
            LocalPosition = localPosition;
            Width = width;
            Height = height;
            RotationMillirad = rotationMillirad;
        }

        public override bool Colliding(Collider collider)
        {
            throw new NotImplementedException();
        }
    }
}
