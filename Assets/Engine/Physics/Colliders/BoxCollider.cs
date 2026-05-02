using System;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public Vector2Int WorldPosition => ParentEntity.Position + LocalCenterPosition;
        public Vector2Int LocalCenterPosition { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int LocalRotationMillirad { get; private set; }
        public int RotationMillirad => ParentEntity.RotationMilli + LocalRotationMillirad;

        public Vector2Int[] WorldVertices { get; private set; }

        public BoxCollider(Vector2Int localPosition, int halfWidth, int halfHeight, int localRotationMillirad) : base()
        {
            LocalCenterPosition = localPosition;
            Width = halfWidth * 2;
            Height = halfHeight * 2;
            LocalRotationMillirad = localRotationMillirad;
        }

        public override bool Colliding(Collider collider)
        {
            if (collider is BoxCollider b)
                return Collision.BoxBoxSAT(b, this).IsCollision;
            else if (collider is CircleCollider c)
                return Collision.BoxCircle(this, c);
            else
                throw new NotImplementedException();
        }

        public override bool Contains(Vector2Int point)
        {
            Vector2Int r = point - WorldVertices[0];
            long sc1 = r.Dot(WorldVertices[1] - WorldVertices[0]);
            long sc2 = r.Dot(WorldVertices[3] - WorldVertices[0]);
            if (sc1 < 0 || sc1 > (WorldVertices[1] - WorldVertices[0]).LengthSquared() || sc2 < 0 || sc2 > (WorldVertices[3] - WorldVertices[0]).LengthSquared())
                return false;
            return true;
        }

        public override void PhysicsUpdate()
        {
            Vector2Int right = Vector2Int.MilliDirectionFromMilliRad(RotationMillirad); //cos multiplied by 1000
            Vector2Int up = right.OrthogonalCounterClockwise();
            int halfWidth = Width / 2; //width and height are divisible by 2 by design
            int halfHeight = Height / 2;

            WorldVertices = new Vector2Int[4]
            {
                WorldPosition + (-right * halfWidth + up * halfHeight) / 1000,
                WorldPosition + (right * halfWidth + up * halfHeight) / 1000,
                WorldPosition + (right * halfWidth - up * halfHeight) / 1000,
                WorldPosition + (-right * halfWidth - up * halfHeight) / 1000,
            };
        }
    }
}
