# Automathon

The Hackathon Game !!! Sorry for being a bit late. The game isn't ready yet and still lacks a lot of polish regarding art and sound, but the main gameplay loop is here, with AI integration. Gameplay will probably receive a lot of tweaks before the actual release.

# Installation

Download the release under the Release tab.

# To play the game, with the view from Unity
- Launch the game in ``AutomathonGame/AutomathonMVC.exe``
- click once to remove the logo and have the input menu
- Choose inputs (controller or IA or left keyboard + Mouse) (you can also use the arrow keys for a second player, but It's recommended to keep this for testing as playing fully with a keyboard is almost impossible)
The game was designed to be played with controllers

### Controller controls:
- Move with left stick
- Aim with right stick
- Machine Gun : right trigger
- Missile : Right bumper
- Shield : Left bumper
- Dash : Left trigger

### Keyboard Controls :
- Move with WASD/ZQSD
- Aim with the mouse
- Machine Gun : Left click
- Missile : Right click
- Shield : E
- Dash : Left shift


# In order to use AI

Install the requirements in PythonAI
```
python -m pip install -r requirements.txt
```

An example AI is given in ``PythonAI/example_ai.py``.

## Fixed point numbers

The simulation is fully deterministic: every physical quantity (positions, sizes, velocities, rotations, health...)
is a fixed point number instead of a float.

- In the game (C#), ``Fixed`` is a Q16.16 number stored in an ``int`` and ``FixedLong`` a Q32.32 number stored in a
  ``long`` (used for wide intermediate results). ``FixedVector2`` is a 2D vector of ``Fixed`` and ``FixedMath`` contains
  ``Sqrt``, ``Sin``, ``Cos``, ``Atan2``, ``Clamp``... computed with integer arithmetic only. They are used like floats:
  ``Fixed speed = 7.5; FixedVector2 velocity = direction.Normalized() * speed;``.
- In Python, the bridge gives you the same values: ``Fixed`` (``bridge/fixed.py``) and ``FixedVector2``
  (``bridge/datatypes.py``). They also behave like floats/vectors (``+ - * /``, comparisons with ints and floats,
  ``float(x)``, ``math.sqrt(x)``, ``v.length()``, ``v.normalized()``, ``v.dot(w)``...) and reproduce the game's
  arithmetic bit for bit. You can build them from floats/ints (``FixedVector2(0.5, 1)``, or simply a ``(0.5, 1)`` tuple
  in an ``AIAction``).
- Units: 1 = 1 Unity unit (the arena is about 27.5 x 15.5 units, centered on (0, 0)), angles are in radians and speeds
  in units per second. Over JSON, values are plain numbers (``{"X": -10, "Y": 0.25}``), the conversion is lossless.

## Training - Headless

Open the headless version of the game ``Headless.exe`` first.
Launch ``example_train.py`` (it is pretty self explanatory and commenteded, you can modify it in order to suit your needs)

⚠ : no pretreatment of data is done (no normalization, or useful data given, just actual raw positions, speeds of entities, as Fixed numbers)

## Playing - With Unity
- Launch ``example_play.py``, it is pretty self explanatory with comments (you just need a function that takes a ``GameState`` and outputs an ``AIAction``)
- Launch the game
- Type the tcp port in the input field (by default it is 5555)
- An AI icon should appear, you can now play with the AI !


# Known issues that will be fixed later : 
- If you launch ``example_play.py`` after launching Unity, Unity doesn't seem to be able to connect to it. You need to always start python first before opening the view of the game.
- The game is ugly
- The game doesn't have sound
