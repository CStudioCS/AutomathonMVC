from bridge.datatypes import *
from math import sqrt

# this is an example of an AI that just goes towards the player and does every action it can as fast as possible

def length_squared(v):
    return v.X * v.X + v.Y * v.Y

def normalize(v: Vector2Int):
    length = sqrt(length_squared(v))
    v.X = int(v.X * 1000 / length)
    v.Y = int(v.Y * 1000 / length)

def decide_action(state: GameState):

    # this is where you do all the AI stuff you want to get an AIAction object using state
    # When running the game not in training mode, this is the only function that will be used
    # Take a state and returns the action the ai takes
    
    # WARNING : running this function should be relatively fast, (like 5ms TOPS) so
    # that the game doesn't look like a slide show

    if state.SelfTank is not None and state.EnemyTank is not None:
        movingDir = Vector2Int(X=0, Y=0)
        aimDir = Vector2Int(X=0, Y=0)

        aimDir.X = state.EnemyTank.Position.X - state.SelfTank.Position.X
        aimDir.Y = state.EnemyTank.Position.Y - state.SelfTank.Position.Y
        forward = length_squared(aimDir) > 4000 * 4000

        normalize(aimDir)
        movingDir = aimDir if forward else Vector2Int(X=-aimDir.X, Y=-aimDir.Y)

        action = AIAction(MovingDirection=movingDir, AimingDirection=aimDir) 

        action.MachineGun = state.SelfTank.MachineGunCooldownFramesLeft == 0 # act if we can (the cooldown is at zero)
        action.Missile = state.SelfTank.MissileCooldownFramesLeft == 0
        action.Shield = state.SelfTank.ShieldCooldownFramesLeft == 0
        action.Dash = state.SelfTank.DashCooldownFramesLeft == 0

        """for e in state.SelfTank:
            match e:
                case TankState() as t:
                    if aimDir.X != 0 or aimDir.Y != 0:
                        aimDir.X = t.Position.X - aimDir.X
                        aimDir.Y = t.Position.Y - aimDir.Y
                        forward = length_squared(aimDir) > 1000 * 1000

                        normalize(aimDir)
                        movingDir = aimDir if forward else Vector2Int(X=-aimDir.X, Y=-aimDir.Y)
                        
                    else:
                        aimDir = t.Position
                        meTank = t
                case _:
                    pass"""
        
    else:
        action = AIAction(MovingDirection=Vector2Int(X=1000, Y=0), AimingDirection=Vector2Int(X=0, Y=1000))
    
    return action