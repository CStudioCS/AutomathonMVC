from datatypes import *

def decide_action(state: GameState):
    # this is where you do all the AI stuff you want to get an AIAction object using state
    # When running the game not in training mode, this is the only function that will be used
    # Take a state and returns the action the ai takes
    
    # WARNING : running this function should be relatively fast, (like 5ms TOPS) so
    # that the game doesn't look like a slide show

    action = AIAction()
    action.MovingDirection = Vector2Int(X=700, Y=500) # for example

    return action