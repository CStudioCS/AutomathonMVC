using Automathon.AI;

namespace HeadlessBridge;

public static class Program
{
    private static TrainingManager gameBridge;
    public static void Main(string[] args)
    {
        string tcpAddress = args.Length == 0 ? "tcp://localhost:5555" : args[0];

        gameBridge = new TrainingManager(tcpAddress);

        while (!gameBridge.Step()) { }

        gameBridge.Dispose();
    }
}