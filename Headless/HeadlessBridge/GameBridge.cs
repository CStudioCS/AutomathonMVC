using Automathon;
using Automathon.Engine;
using Automathon.Game;

namespace HeadlessBridge;

public class GameBridge
{
    private TrainingAIInputProvider inputProvider1;
    private TrainingAIInputProvider inputProvider2;

    public void InitializeGame()
    {
        GameplayManager.Initialize();

        inputProvider1 = new TrainingAIInputProvider();
        inputProvider2 = new TrainingAIInputProvider();

        GameplayManager.Instantiate(new Tank(new Vector2Int(-5000, 0), inputProvider1));
        GameplayManager.Instantiate(new Tank(new Vector2Int(5000, 0), inputProvider2));
    }

    public float[] Step(float[] action1, float[] action2)
    {
        inputProvider1.UpdateFromAction(action1);
        inputProvider2.UpdateFromAction(action2);

        GameplayManager.Update();

        return GameplayManager.GetState();
    }

    public void Dispose()
    {
        GameplayManager.Dispose();
    }
}
