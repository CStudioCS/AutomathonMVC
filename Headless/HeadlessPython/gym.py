import grpc
import proto_pb2 as proto_pb2
import proto_pb2_grpc as proto_pb2_grpc


class Gym:
    def __init__(self):
        self.channel = grpc.insecure_channel("localhost:50051")
        self.stub = proto_pb2_grpc.GymServiceStub(self.channel)

    def step(self, action: list[float]):
        step_response: proto_pb2.StepResponse = self.stub.Step(proto_pb2.Action(action=[5.0, 2.0, 3.0]))
        return step_response.observation, step_response.done

    def reset(self):
        self.stub.Reset(proto_pb2.Empty())