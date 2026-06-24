import zmq
import time
import json
from dataclasses import dataclass, asdict, field

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

poller = zmq.Poller()
poller.register(socket, zmq.POLLIN)

print("Python Side launched")

@dataclass
class Vector2:
    X: int
    Y: int

@dataclass
class AIAction:
    MovingDirection: Vector2 = field(default_factory=lambda: Vector2(1000, 0))
    AimingDirection: Vector2 = field(default_factory=lambda: Vector2(0, 1000))
    MachineGun: bool = False
    Missile: bool = False
    Shield: bool = False
    Dash: bool = False

def update():
    if poller.poll(500):
        state = socket.recv_string()
        json.loads(state)
        s = json.dumps(asdict(AIAction()))
        socket.send_string(s)


try:
    while True:
        update()
except KeyboardInterrupt:
    print("Shutting down Python side")