from pydantic import BaseModel, Field
from typing import Union, List, Annotated, Literal

# Define your base structures
class Vector2Int(BaseModel):
    X: int
    Y: int

class StateBase(BaseModel):
    Position: Vector2Int

# Define your subclasses
class BulletState(StateBase):
    Type: Literal["BulletState"] = "BulletState"
    Radius: int
    Velocity: Vector2Int

class MissileState(StateBase):
    Type: Literal["MissileState"] = "MissileState"
    Radius: int
    Velocity: Vector2Int

class BaseWallState(StateBase):
    Size: Vector2Int
    RotationMilli: int

class WallState(BaseWallState):
    Type: Literal["WallState"] = "WallState"

class ShieldState(BaseWallState):
    Type: Literal["ShieldState"] = "ShieldState"
    Velocity: Vector2Int

class TankState(StateBase):
    Type: Literal["TankState"] = "TankState"
    Width: int
    Height: int
    Velocity: Vector2Int
    Health: int
    ShieldCooldownFramesLeft: int
    MissileCooldownFramesLeft: int
    MachineGunCooldownFramesLeft: int
    DashCooldownFramesLeft: int

State = Annotated[
    Union[BulletState, MissileState, WallState, ShieldState, TankState], 
    Field(discriminator='Type')
]

class GameState(BaseModel):
    States: List[State]
    Done: bool