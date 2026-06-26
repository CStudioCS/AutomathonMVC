import zmq
import json
from datatypes import *
from collections.abc import Callable

class Play:
    def __init__(self, decide_action_function: Callable[[GameState], AIAction]):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REP)
        self.socket.bind("tcp://localhost:5555")

        self.poller = zmq.Poller()
        self.poller.register(self.socket, zmq.POLLIN)

        self.decide_action = decide_action_function

    def respond(self, timeout: int | None=500):
        if self.poller.poll(timeout):
            state = self.__receive_state__()
            action = self.decide_action(state)
            self.__send_action__(AIMessage(Reset=False, DoneWithTraining=False, SelfAction=action, EnemyAction=None))
    
    def __receive_state__(self):
        state_string = self.socket.recv_string()
        raw_dict = json.loads(state_string)
        return GameState(**raw_dict)
    
    def __send_action__(self, msg: AIMessage):
        s = msg.model_dump_json()
        self.socket.send_string(s)