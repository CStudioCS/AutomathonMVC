import zmq

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.connect("tcp://localhost:5555")

while True:
    state = socket.recv_string()
    socket.send_string("Hellooo !!! IT WOOORKS :O")