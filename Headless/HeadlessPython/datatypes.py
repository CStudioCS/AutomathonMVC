from dataclasses import dataclass, field

@dataclass
class Vector2:
    X: int
    Y: int

@dataclass
class AIAction:
    MovingDirection: Vector2 = field(default_factory=lambda: Vector2(1000, 0))
    AimingDirection: Vector2 = field(default_factory=lambda: Vector2(0, 1000))
    MachineGun: bool = False
    Missile: bool = False
    Shield: bool = False
    Dash: bool = False

