import zmq
import time
import json

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

poller = zmq.Poller()
poller.register(socket, zmq.POLLIN)

print("connected")

def update():
    if poller.poll(500):
        state = socket.recv_string()
        json.loads(state)
        socket.send_string(str(time.time()))

        
try:
    while True:
        update()
except KeyboardInterrupt:
    print("Shutting down Python side")