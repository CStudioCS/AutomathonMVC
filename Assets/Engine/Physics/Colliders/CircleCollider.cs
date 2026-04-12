namespace Automathon.Engine.Physics
{
    public class CircleCollider : Collider
    {
        public Vector2Int LocalPosition;
        public int Radius;

        public CircleCollider(Vector2Int localPosition, int radius)
        {
            LocalPosition = localPosition;
            Radius = radius;
        }

        public override bool Colliding(Collider collider)
        {
            throw new System.NotImplementedException();
        }
    }
}
