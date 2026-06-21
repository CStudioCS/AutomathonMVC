using Automathon;
using Automathon.Game.Input;

namespace HeadlessBridge;

public class TrainingAIInputProvider : IInputProvider
{
    public void UpdateFromAction(float[] actionVector)
    {

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
