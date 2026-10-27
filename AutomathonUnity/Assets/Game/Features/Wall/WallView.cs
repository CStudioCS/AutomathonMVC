using Automathon;
using Automathon.Game;
using UnityEngine;

public class WallView : EntityView<Wall>
{
    [SerializeField] private NeonZapper zapper;
    private Wall wall;

    public override void Initialize(Wall wall)
    {
        base.Initialize(wall);
        this.wall = wall;

        zapper.SetSize(new Vector2(
            wall.BoxCollider.Width.ToWorldLength(),
            wall.BoxCollider.Height.ToWorldLength()));

        wall.OnHit += HandleHit;
    }

    protected override void OnDestroy()
    {
        if (wall != null) wall.OnHit -= HandleHit;
        base.OnDestroy();
    }

    private void HandleHit(FixedVector2 collisionPos, int power)
    {
        FixedVector2 d = collisionPos - wall.Position;

        bool horizontal = wall.Size.X >= wall.Size.Y;   // >= pour matcher le shader
        Fixed length = horizontal ? wall.Size.X : wall.Size.Y;

        // Grand axe unitaire
        FixedVector2 right = FixedVector2.FromAngle(wall.Rotation);
        FixedVector2 dir = horizontal ? right : right.OrthogonalCounterClockwise();

        // Position of the hit along the long axis, 0..1000 (0 = the -right/-up end, 1000 = the other)
        Fixed alongAxis = d.Dot(dir) / length; // in [-0.5, 0.5] when on the wall
        int t = Mathf.Clamp((alongAxis * 1000).RoundToInt() + 500, 0, 1000);

        zapper.Impact(t, power);
    }
}
