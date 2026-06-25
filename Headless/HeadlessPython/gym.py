import zmq
import json
from dataclasses import asdict
from datatypes import *
from states import *

class Gym:
    def __init__(self):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REP)
        self.socket.bind("tcp://*:5555")

        self.poller = zmq.Poller()
        self.poller.register(self.socket, zmq.POLLIN)

    def step(self, action: AIAction, timeout=500):

        if self.poller.poll(timeout):
            state_string = self.socket.recv_string()
            raw_dict = json.loads(state_string)

            s = json.dumps(asdict(action))
            self.socket.send_string(s)

            return GameState(**raw_dict)
        
        raise TimeoutError()

    def reset(self) -> GameState:
        raise NotImplementedError #ask the game to reset, return state