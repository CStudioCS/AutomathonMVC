"""
Data exchanged with the game.

Every physical quantity (positions, sizes, velocities, rotations, health...) is a :class:`Fixed`
(Q16.16 fixed point number, see ``bridge/fixed.py``) and vectors are :class:`FixedVector2`.
They behave like floats/2D vectors, and the values are exactly the ones used by the game simulation.

Units: 1 = 1 world unit (1 Unity unit), angles are in radians, velocities in units per second.
The map is about 27.5 x 15.5 units, centered on (0, 0).
"""

import math
from typing import Annotated, Any, List, Literal, Union

from pydantic import BaseModel, ConfigDict, Field, model_validator

from bridge.fixed import Fixed, _rounded_divide, _saturate, FRACTIONAL_BITS, ONE_RAW


class FixedVector2(BaseModel):
    """
    2D vector of Fixed numbers. Can be built with ``FixedVector2(1, 0.5)``, ``FixedVector2(X=1, Y=0.5)`` or from a
    ``(x, y)`` tuple anywhere a FixedVector2 is expected. Assigning floats/ints to X or Y converts them to Fixed.

    Supports ``+``, ``-``, ``* scalar``, ``/ scalar``, ``dot``, ``cross``, ``length``, ``normalized``...
    ``length``, ``length_squared``, ``dot``, ``cross`` and ``normalized`` give exactly the same results as the game.
    """

    model_config = ConfigDict(validate_assignment=True)

    X: Fixed = Fixed.ZERO
    Y: Fixed = Fixed.ZERO

    def __init__(self, *args: Any, **data: Any):
        if args:
            if len(args) == 1 and isinstance(args[0], (tuple, list)):
                args = tuple(args[0])
            if len(args) != 2:
                raise TypeError("FixedVector2 takes 2 positional arguments (x, y)")
            data.setdefault("X", args[0])
            data.setdefault("Y", args[1])
        super().__init__(**data)

    @model_validator(mode="before")
    @classmethod
    def _from_sequence(cls, data: Any) -> Any:
        if isinstance(data, (tuple, list)) and len(data) == 2:
            return {"X": data[0], "Y": data[1]}
        return data

    # ------------------------------------------------------------------ constructors

    @classmethod
    def zero(cls) -> "FixedVector2":
        return cls(0, 0)

    @classmethod
    def from_angle(cls, radians: Any) -> "FixedVector2":
        """Unit vector (cos, sin) of an angle in radians (within 1/65536 of the game's CORDIC implementation)."""
        a = float(radians)
        return cls(math.cos(a), math.sin(a))

    # ------------------------------------------------------------------ operators

    def __add__(self, other: "FixedVector2") -> "FixedVector2":
        if not isinstance(other, FixedVector2):
            return NotImplemented
        return FixedVector2(self.X + other.X, self.Y + other.Y)

    def __sub__(self, other: "FixedVector2") -> "FixedVector2":
        if not isinstance(other, FixedVector2):
            return NotImplemented
        return FixedVector2(self.X - other.X, self.Y - other.Y)

    def __neg__(self) -> "FixedVector2":
        return FixedVector2(-self.X, -self.Y)

    def __mul__(self, scalar: Any) -> "FixedVector2":
        if isinstance(scalar, FixedVector2):
            return NotImplemented
        return FixedVector2(self.X * scalar, self.Y * scalar)

    __rmul__ = __mul__

    def __truediv__(self, scalar: Any) -> "FixedVector2":
        if isinstance(scalar, FixedVector2):
            return NotImplemented
        return FixedVector2(self.X / scalar, self.Y / scalar)

    def __str__(self) -> str:
        return f"({self.X}, {self.Y})"

    def __repr__(self) -> str:
        return f"FixedVector2({self.X}, {self.Y})"

    # ------------------------------------------------------------------ math (bit exact with the game)

    def dot(self, other: "FixedVector2") -> Fixed:
        exact = self.X.raw * other.X.raw + self.Y.raw * other.Y.raw  # Q32.32
        return Fixed.from_raw((exact + (1 << 15)) >> 16)

    def cross(self, other: "FixedVector2") -> Fixed:
        """X * other.Y - Y * other.X"""
        exact = self.X.raw * other.Y.raw - self.Y.raw * other.X.raw
        return Fixed.from_raw((exact + (1 << 15)) >> 16)

    def length_squared(self) -> Fixed:
        return self.dot(self)

    def length(self) -> Fixed:
        n = self.X.raw * self.X.raw + self.Y.raw * self.Y.raw
        root = math.isqrt(n)
        if n - root * root > root:
            root += 1
        return Fixed.from_raw(root)

    def normalized(self) -> "FixedVector2":
        """Unit vector with the same direction (the zero vector stays zero)."""
        x, y = self.X.raw, self.Y.raw
        if x == 0 and y == 0:
            return FixedVector2(0, 0)
        max_abs = max(abs(x), abs(y))
        while max_abs < (1 << 29):
            x <<= 1
            y <<= 1
            max_abs <<= 1
        n = x * x + y * y
        length = math.isqrt(n)
        if n - length * length > length:
            length += 1
        return FixedVector2(
            Fixed.from_raw(_rounded_divide(x << FRACTIONAL_BITS, length)),
            Fixed.from_raw(_rounded_divide(y << FRACTIONAL_BITS, length)),
        )

    def distance_to(self, other: "FixedVector2") -> Fixed:
        return (self - other).length()

    def angle(self) -> Fixed:
        """Angle in radians in ]-pi, pi] (within 1/65536 of the game's CORDIC implementation)."""
        return Fixed(math.atan2(float(self.Y), float(self.X)))

    def rotated(self, radians: Any) -> "FixedVector2":
        c, s = math.cos(float(radians)), math.sin(float(radians))
        x, y = float(self.X), float(self.Y)
        return FixedVector2(x * c - y * s, x * s + y * c)

    def to_tuple(self) -> tuple:
        """(x, y) as floats."""
        return float(self.X), float(self.Y)


# Shorter alias
Vector2 = FixedVector2


class StateBase(BaseModel):
    Position: FixedVector2


class BulletState(StateBase):
    Type: Literal["BulletState"] = "BulletState"
    Radius: Fixed
    Velocity: FixedVector2


class MissileState(StateBase):
    Type: Literal["MissileState"] = "MissileState"
    Radius: Fixed
    Rotation: Fixed = Fixed.ZERO
    Velocity: FixedVector2


class BaseWallState(StateBase):
    Size: FixedVector2
    Rotation: Fixed  # radians


class WallState(BaseWallState):
    Type: Literal["WallState"] = "WallState"


class ShieldState(BaseWallState):
    Type: Literal["ShieldState"] = "ShieldState"
    Velocity: FixedVector2
    Health: Fixed


class TankState(StateBase):
    Type: Literal["TankState"] = "TankState"
    Width: Fixed
    Height: Fixed
    Radius: Fixed = Fixed(0.419)  # collision radius
    Rotation: Fixed = Fixed.ZERO  # radians, direction the body faces
    Velocity: FixedVector2
    AimingDirection: FixedVector2 = Field(default_factory=lambda: FixedVector2(1, 0))  # unit vector
    Health: Fixed
    ShieldCooldownFramesLeft: int
    MissileCooldownFramesLeft: int
    MachineGunCooldownFramesLeft: int
    DashCooldownFramesLeft: int


State = Annotated[
    Union[BulletState, MissileState, WallState, ShieldState, TankState],
    Field(discriminator="Type"),
]


class GameState(BaseModel):
    SelfTank: TankState | None = None
    EnemyTank: TankState | None = None
    BulletStates: List[BulletState] = Field(default_factory=lambda: [])
    MissileStates: List[MissileState] = Field(default_factory=lambda: [])
    WallStates: List[WallState] = Field(default_factory=lambda: [])
    ShieldStates: List[ShieldState] = Field(default_factory=lambda: [])
    Done: bool


class AIAction(BaseModel):
    """
    MovingDirection: vectors longer than 1 are normalized by the game, shorter ones make the tank move slower.
    AimingDirection: normalized by the game, a zero vector keeps the previous aiming direction.
    Both accept FixedVector2, (x, y) tuples, or dicts. Values are Fixed (floats/ints are converted).
    """

    MovingDirection: FixedVector2 = Field(default_factory=lambda: FixedVector2(1, 0))
    AimingDirection: FixedVector2 = Field(default_factory=lambda: FixedVector2(0, 1))
    MachineGun: bool = False
    Missile: bool = False
    Shield: bool = False
    Dash: bool = False


class AIMessage(BaseModel):
    Reset: bool
    DoneWithTraining: bool
    SelfAction: AIAction | None = None
    EnemyAction: AIAction | None = None


__all__ = [
    "Fixed", "FixedVector2", "Vector2", "StateBase", "BulletState", "MissileState", "BaseWallState", "WallState",
    "ShieldState", "TankState", "State", "GameState", "AIAction", "AIMessage",
]
