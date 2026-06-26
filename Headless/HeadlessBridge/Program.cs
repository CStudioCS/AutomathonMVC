using Automathon.AI;

namespace HeadlessBridge;

public static class Program
{
    private static TrainingManager gameBridge;
    public static void Main(string[] args)
    {
        gameBridge = new TrainingManager(args[0]);

        while (!gameBridge.Step()) { }

        gameBridge.Dispose();
    }
}