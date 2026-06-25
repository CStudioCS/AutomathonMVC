import zmq
import json
from gym import Gym
from datatypes import *
from states import *

env = Gym()
print("Python Side launched")

def update():
    state = env.step(AIAction())
    print(state.States)
    print(state.Done)

try:
    while True:
        update()
        break
except KeyboardInterrupt:
    print("Shutting down Python side")