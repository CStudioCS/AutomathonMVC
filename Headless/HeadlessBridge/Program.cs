namespace HeadlessBridge;

public static class Program
{
    private static GameBridge gameBridge;
    public static void Main(string[] args)
    {
        gameBridge = new GameBridge();
        gameBridge.InitializeGame();

        while (gameBridge.Step()) { }

        gameBridge.Dispose();
    }
}