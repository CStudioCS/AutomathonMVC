using Automathon;
using Automathon.AI;
using Automathon.Engine;
using Automathon.Game;
using System;

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

        GameplayManager.Instantiate(new Tank(new Vector2Int(-5000, 0), inputProvider1));
        GameplayManager.Instantiate(new Tank(new Vector2Int(5000, 0), inputProvider2));
    }

    public bool Step()
    {
        if (ServerHandler.GetAIResponse(out string response))
        {
            //inputProvider1.UpdateFromAction();
            //inputProvider2.UpdateFromAction();
            Debug.Log("Received a response");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Log("Didn't received a response");
            Console.ForegroundColor = ConsoleColor.White;
        }

        GameplayManager.Update();

        return true;
    }

    public void Dispose()
    {
        GameplayManager.Dispose();
    }
}
