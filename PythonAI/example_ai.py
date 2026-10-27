from bridge.datatypes import *

# this is an example of an AI that just goes towards the player and does every action it can as fast as possible
#
# Every number coming from the game is a Fixed (fixed point number) and every vector a FixedVector2.
# They work like floats / 2D vectors: you can add, multiply, compare them with ints and floats,
# pass them to math.sqrt, convert them with float(...), etc.
# Distances are in world units (the map is ~27.5 x 15.5 units), angles in radians, speeds in units per second.

def decide_action(state: GameState):

    # this is where you do all the AI stuff you want to get an AIAction object using state
    # When running the game not in training mode, this is the only function that will be used
    # Take a state and returns the action the ai takes
    
    # WARNING : running this function should be relatively fast, (like 5ms TOPS) so
    # that the game doesn't look like a slide show

    if state.SelfTank is not None and state.EnemyTank is not None:
        to_enemy = state.EnemyTank.Position - state.SelfTank.Position
        forward = to_enemy.length() > 4  # go towards the enemy until we're 4 units away, then back off

        aim_dir = to_enemy.normalized()
        moving_dir = aim_dir if forward else -aim_dir

        action = AIAction(MovingDirection=moving_dir, AimingDirection=aim_dir)

        action.MachineGun = state.SelfTank.MachineGunCooldownFramesLeft == 0 # act if we can (the cooldown is at zero)
        action.Missile = state.SelfTank.MissileCooldownFramesLeft == 0
        action.Shield = state.SelfTank.ShieldCooldownFramesLeft == 0
        action.Dash = state.SelfTank.DashCooldownFramesLeft == 0
        
    else:
        action = AIAction(MovingDirection=FixedVector2(1, 0), AimingDirection=FixedVector2(0, 1))
    
    return action
