using System;

namespace Automathon.Engine.Physics
{
    public class BoxCollider : Collider
    {
        public Vector2Int WorldPosition => ParentEntity.Position + LocalCenterPosition;
        public Vector2Int LocalCenterPosition { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int RotationMillirad { get; set; }

        public Vector2Int[] Coords { get; private set; }

        public BoxCollider(Vector2Int localPosition, int halfWidth, int halfHeight, int rotationMillirad) : base()
        {
            LocalCenterPosition = localPosition;
            Width = halfWidth * 2;
            Height = halfHeight * 2;
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

        public override bool Contains(Vector2Int point)
        {
            Vector2Int r = point - Coords[0];
            float sc1 = r.Dot(Coords[1] - Coords[0]);
            float sc2 = r.Dot(Coords[3] - Coords[0]);
            if (sc1 < 0 || sc1 > (Coords[1] - Coords[0]).LengthSquared() || sc2 < 0 || sc2 > (Coords[3] - Coords[0]).LengthSquared())
                return false;
            return true;
        }

        public override void PhysicsUpdate()
        {
            Vector2Int right = Vector2Int.MilliDirectionFromMilliRad(RotationMillirad); //cos multiplied by 1000
            Vector2Int up = right.OrthogonalCounterClockwise();
            int halfWidth = Width / 2; //width and height are divisible by 2 by design
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
