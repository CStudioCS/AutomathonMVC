import zmq
import time

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

poller = zmq.Poller()
poller.register(socket, zmq.POLLIN)

print("connected")

def update():
    if poller.poll(500):
        print("try receive")
        state = socket.recv_string()
        print("received")
        socket.send_string(str(time.time()))
        print("state delivered")

try:
    while True:
        update()
except KeyboardInterrupt:
    print("Shutting down Python side")