using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeadlessBridge;

public static class Program
{
    private static GameBridge gameBridge
        ;
    private const int PORT = 50051;
    public static void Main(string[] args)
    {
        gameBridge = new GameBridge();
        gameBridge.InitializeGame();
        SetupServer().Wait();
    }

    private async static Task SetupServer()
    {
        Server server = new Server
        {
            Services = { GymService.BindService(new Gym(gameBridge)) },
            Ports = { new ServerPort("localhost", PORT, ServerCredentials.Insecure) }
        };

        server.Start();

        Console.WriteLine($"gRPC server listening on port {PORT}");


        Console.WriteLine("Press Ctrl+C to stop...");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // prevent immediate process termination
            cts.Cancel();
        };


        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Stopping...");
        }

        server.ShutdownAsync().Wait();

        gameBridge.Dispose();
    }
}