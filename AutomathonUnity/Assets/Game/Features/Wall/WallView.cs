using Automathon.Game;
using Automathon.Utility;
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
            wall.BoxCollider.Width / (float)WorldConstants.SPACE_SCALE,
            wall.BoxCollider.Height / (float)WorldConstants.SPACE_SCALE));

        wall.OnHit += HandleHit;
    }

    protected override void OnDestroy()
    {
        if (wall != null) wall.OnHit -= HandleHit;
        base.OnDestroy();
    }

    private void HandleHit(Automathon.Vector2Int collisionPos, int power)
    {
        Automathon.Vector2Int d = collisionPos - wall.Position;

        bool horizontal = wall.Size.X >= wall.Size.Y;   // >= pour matcher le shader
        int L = horizontal ? wall.Size.X : wall.Size.Y;

        // Grand axe unitaire, à l'échelle 1000 de TrigTable
        Automathon.Vector2Int dir = horizontal
            ? new Automathon.Vector2Int(TrigTable.Cos(wall.RotationMilli), TrigTable.Sin(wall.RotationMilli))
            : new Automathon.Vector2Int(-TrigTable.Sin(wall.RotationMilli), TrigTable.Cos(wall.RotationMilli));


        int dot = d.X * dir.X + d.Y * dir.Y;
        int t = Mathf.Clamp(500 + dot / L, 0, 1000);

        zapper.Impact(t, power);
    }
}