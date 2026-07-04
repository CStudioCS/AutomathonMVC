from example_ai import decide_action
from bridge.play import Play
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--port", default="5555", help="The local tcp port the AI will use to communicate with the game")
args = parser.parse_args()

play = Play(decide_action, tcp_port=args.port)

try:
    while True:
        play.respond()
except KeyboardInterrupt:
    print("Play server stopped.")