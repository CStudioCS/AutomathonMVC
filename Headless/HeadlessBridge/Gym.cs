using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HeadlessBridge;

public class Gym : GymService.GymServiceBase
{
    private GameBridge gameBridge;

    public Gym(GameBridge gameBridge)
    {
        this.gameBridge = gameBridge;
        Console.WriteLine($"Gym service initialized");
    }

    public override Task<StepResponse> Step(Action request, ServerCallContext context)
    {
        List<float> action1 = [.. request.Action1];
        List<float> action2 = [.. request.Action2];

        float[] state = gameBridge.Step(action1.ToArray(), action2.ToArray());

        Console.WriteLine($"RECEIVED STEP REQUEST");
        return Task.FromResult(new StepResponse()
        {
            Observation = { state },
            Done = false
        });
    }

    public override Task<Empty> Reset(Empty request, ServerCallContext context)
    {
        Console.WriteLine($"RECEIVED RESET REQUEST");
        //TODO: Reset the game here
        return Task.FromResult(new Empty());
    }
}
