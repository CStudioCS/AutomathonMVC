using Automathon;
using Automathon.AI;
using Automathon.Engine;
using Automathon.Game;

namespace HeadlessBridge;

public class GameBridge
{
    private AIInputProvider inputProvider1;
    private AIInputProvider inputProvider2;

    public void InitializeGame()
    {
        GameplayManager.Initialize();

        inputProvider1 = new AIInputProvider();
        inputProvider2 = new AIInputProvider();

        ServerHandler.StartServer();

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
        ServerHandler.StopServer();
        GameplayManager.Dispose();
    }
}
