from dumbassAI import decide_action
from play import Play

play = Play(decide_action)

while True:
    play.respond()