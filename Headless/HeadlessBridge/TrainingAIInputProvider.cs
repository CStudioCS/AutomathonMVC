using Automathon;
using Automathon.Game.Input;

namespace HeadlessBridge;

public class TrainingAIInputProvider : IInputProvider
{
    private Vector2Int movingDir = new Vector2Int(1000, 0);
    private Vector2Int aimingDir = new Vector2Int(1000, 0);
    public void UpdateFromAction(float[] actionVector)
    {


        //Attention : Data du user donc programmer defensivement pour eviter les erreurs
        Vector2Int move = new Vector2Int((int)(actionVector[0] * 1000), (int)(actionVector[1] * 1000));
        if (move != Vector2Int.Zero)
        {
            move.NormalizeAtScale(1000);
            movingDir = move;
        }

        Vector2Int aim = new Vector2Int((int)(actionVector[2] * 1000), (int)(actionVector[3] * 1000));
        if (aim != Vector2Int.Zero)
        {
            aim.NormalizeAtScale(1000);
            aimingDir = aim;
        }


    }

    public Vector2Int GetMilliAimingDir()
    {
        throw new System.NotImplementedException();
    }

    public Vector2Int GetMilliMovementDir()
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldDash()
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldMissile()
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldShield()
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldShoot()
    {
        throw new System.NotImplementedException();
    }
}
