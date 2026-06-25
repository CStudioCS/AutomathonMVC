import zmq
import json
from datatypes import *

class Gym:
    def __init__(self):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REP)
        self.socket.bind("tcp://*:5555")

        self.poller = zmq.Poller()
        self.poller.register(self.socket, zmq.POLLIN)

    def step(self, action: AIAction, timeout=500):

        if self.poller.poll(timeout):
            state = self.__receive_state__()
            self.__send_action__(AIMessage(Reset=False, Action=action))

            return state
        
        raise TimeoutError()

    def reset(self) -> GameState:
        self.__send_action__(AIMessage(Reset=False, Action=None))

        state_string = self.socket.recv_string()
        raw_dict = json.loads(state_string)
        return GameState(**raw_dict) #ask the game to reset, return state

    def __receive_state__(self):
        state_string = self.socket.recv_string()
        raw_dict = json.loads(state_string)
        return GameState(**raw_dict)
    
    def __send_action__(self, msg: AIMessage):
        s = msg.model_dump_json()
        self.socket.send_string(s)