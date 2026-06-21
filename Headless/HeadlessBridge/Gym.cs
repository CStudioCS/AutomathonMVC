using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace HeadlessBridge;

public class Gym : GymService.GymServiceBase
{
    public override Task<StepResponse> Step(Action request, ServerCallContext context)
    {
        Console.WriteLine($"RECEIVED STEP REQUEST");
        return Task.FromResult(new StepResponse()
        {
            Observation = { 1.0f, request.Action_[0], 0.0f },
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
