from dumbassAI import decide_action
from play import Play

play = Play(decide_action)

try:
    while True:
        play.respond()
except KeyboardInterrupt:
    print("Play server stopped.")