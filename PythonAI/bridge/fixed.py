"""
Fixed point numbers, mirroring the C# ``Automathon.Fixed`` type used by the game.

A :class:`Fixed` is a Q16.16 number: a 32-bit integer ``raw`` that represents ``raw / 65536``.
Every arithmetic operation reproduces the C# implementation bit for bit (same rounding, same saturation),
so values computed on the python side are exactly the values the game would compute.

It is meant to be used like a float::

    from bridge.fixed import Fixed
    a = Fixed(1.5)            # from float, int, str or Decimal
    b = a * 2 + 0.25          # mixes freely with ints and floats -> Fixed
    a < 2, float(b), int(b)   # comparisons and conversions work as expected
    math.sqrt(b)              # anything accepting a float accepts a Fixed (returns a float)
    b.sqrt()                  # exact fixed point square root (same result as the game)

Range is [-32768, 32767.99998] with a resolution of 1/65536 (~0.0000153).
Results that are out of range saturate to Fixed.MAX / Fixed.MIN (and x / 0 saturates as well) instead of wrapping.
"""

from __future__ import annotations

import math
import numbers
from decimal import Decimal, ROUND_HALF_UP
from typing import Any

FRACTIONAL_BITS = 16
ONE_RAW = 1 << FRACTIONAL_BITS
_HALF_RAW = ONE_RAW >> 1
_FRACTION_MASK = ONE_RAW - 1
_RAW_MAX = 2**31 - 1
_RAW_MIN = -(2**31)


def _saturate(raw: int) -> int:
    if raw > _RAW_MAX:
        return _RAW_MAX
    if raw < _RAW_MIN:
        return _RAW_MIN
    return raw


def _trunc_div(n: int, d: int) -> int:
    """Integer division truncating towards zero (like C#)."""
    q = abs(n) // abs(d)
    return q if (n >= 0) == (d >= 0) else -q


def _rounded_divide(n: int, d: int) -> int:
    """Integer division rounding halfway values away from zero (same as the C# Fixed.RoundedDivide)."""
    if d < 0:
        n, d = -n, -d
    half = d // 2
    return _trunc_div(n + half, d) if n >= 0 else _trunc_div(n - half, d)


def _raw_from_float(value: float) -> int:
    if math.isnan(value):
        return 0
    scaled = value * ONE_RAW
    if scaled >= _RAW_MAX:
        return _RAW_MAX
    if scaled <= _RAW_MIN:
        return _RAW_MIN
    # round half away from zero, exactly (math.modf doesn't lose precision)
    frac, integer = math.modf(abs(scaled))
    rounded = int(integer) + (1 if frac >= 0.5 else 0)
    return rounded if scaled >= 0 else -rounded


def _raw_from_decimal(value: Decimal) -> int:
    scaled = (value * ONE_RAW).quantize(Decimal(1), rounding=ROUND_HALF_UP)  # half away from zero
    return _saturate(int(scaled))


class Fixed:
    """Q16.16 fixed point number (see module documentation)."""

    __slots__ = ("_raw",)

    FRACTIONAL_BITS = FRACTIONAL_BITS
    ONE_RAW = ONE_RAW

    # populated after the class definition
    ZERO: "Fixed"
    ONE: "Fixed"
    HALF: "Fixed"
    MAX: "Fixed"
    MIN: "Fixed"
    EPSILON: "Fixed"
    PI: "Fixed"
    HALF_PI: "Fixed"
    TWO_PI: "Fixed"

    def __init__(self, value: Any = 0):
        self._raw = Fixed._to_raw(value)

    # ------------------------------------------------------------------ construction

    @staticmethod
    def _to_raw(value: Any) -> int:
        if isinstance(value, Fixed):
            return value._raw
        if isinstance(value, bool):
            return int(value) << FRACTIONAL_BITS
        if isinstance(value, numbers.Integral):
            return _saturate(int(value) << FRACTIONAL_BITS)
        if isinstance(value, Decimal):
            return _raw_from_decimal(value)
        if isinstance(value, str):
            try:
                return _raw_from_decimal(Decimal(value.strip()))
            except Exception:
                raise ValueError(f"'{value}' is not a valid Fixed number") from None
        if isinstance(value, numbers.Real):
            return _raw_from_float(float(value))
        raise TypeError(f"Cannot convert {type(value).__name__} to Fixed")

    @classmethod
    def from_raw(cls, raw: int) -> "Fixed":
        """Builds a Fixed from its underlying Q16.16 integer (raw / 65536)."""
        result = cls.__new__(cls)
        result._raw = _saturate(int(raw))
        return result

    @classmethod
    def ratio(cls, numerator: int, denominator: int) -> "Fixed":
        """Exact numerator / denominator rounded to the nearest Fixed, ex: Fixed.ratio(1, 3)."""
        if denominator == 0:
            return cls.MAX if numerator > 0 else cls.MIN if numerator < 0 else cls.ZERO
        return cls.from_raw(_rounded_divide(int(numerator) << FRACTIONAL_BITS, int(denominator)))

    @property
    def raw(self) -> int:
        """The underlying Q16.16 integer."""
        return self._raw

    @staticmethod
    def _coerce(other: Any) -> "Fixed | None":
        if isinstance(other, Fixed):
            return other
        if isinstance(other, (numbers.Real, Decimal)):  # int, float, bool, numpy scalars, Decimal...
            return Fixed(other)
        return None  # let python raise the usual TypeError (like float + "str")

    # ------------------------------------------------------------------ conversions

    def __float__(self) -> float:
        return self._raw / ONE_RAW  # exact

    def __int__(self) -> int:
        """Truncates towards zero, like int(float)."""
        return _trunc_div(self._raw, ONE_RAW)

    def __trunc__(self) -> int:
        return int(self)

    def __floor__(self) -> int:
        return self._raw >> FRACTIONAL_BITS

    def __ceil__(self) -> int:
        return -((-self._raw) >> FRACTIONAL_BITS)

    def __round__(self, ndigits: int | None = None):
        """Same semantics as round() on a float (the conversion to float is exact)."""
        if ndigits is None:
            return round(float(self))
        return Fixed(round(float(self), ndigits))

    def __bool__(self) -> bool:
        return self._raw != 0

    def to_decimal(self) -> Decimal:
        """Exact decimal value."""
        return Decimal(self._raw) / ONE_RAW

    def round_to_int(self) -> int:
        """Rounds halfway values away from zero (same as the C# Fixed.RoundToInt)."""
        return _rounded_divide(self._raw, ONE_RAW)

    # ------------------------------------------------------------------ arithmetic (bit exact with C#)

    def __neg__(self) -> "Fixed":
        return Fixed.from_raw(-self._raw)

    def __pos__(self) -> "Fixed":
        return self

    def __abs__(self) -> "Fixed":
        return Fixed.from_raw(abs(self._raw))

    def __add__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return Fixed.from_raw(self._raw + o._raw)

    __radd__ = __add__

    def __sub__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return Fixed.from_raw(self._raw - o._raw)

    def __rsub__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return o - self

    def __mul__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return Fixed.from_raw((self._raw * o._raw + _HALF_RAW) >> FRACTIONAL_BITS)

    __rmul__ = __mul__

    def __truediv__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        if o._raw == 0:
            return Fixed.MAX if self._raw > 0 else Fixed.MIN if self._raw < 0 else Fixed.ZERO
        return Fixed.from_raw(_rounded_divide(self._raw << FRACTIONAL_BITS, o._raw))

    def __rtruediv__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return o / self

    def __floordiv__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        if o._raw == 0:
            raise ZeroDivisionError("Fixed floor division by zero")
        return Fixed(self._raw // o._raw)  # floor of the exact quotient

    def __rfloordiv__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return o // self

    def __mod__(self, other: Any) -> "Fixed":
        """Python semantics (result has the sign of the divisor), like float % float."""
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        if o._raw == 0:
            raise ZeroDivisionError("Fixed modulo by zero")
        return Fixed.from_raw(self._raw % o._raw)

    def __rmod__(self, other: Any) -> "Fixed":
        o = Fixed._coerce(other)
        if o is None:
            return NotImplemented
        return o % self

    def __divmod__(self, other: Any):
        return self // other, self % other

    def __pow__(self, exponent: Any) -> "Fixed":
        return Fixed(float(self) ** float(exponent))

    def __rpow__(self, base: Any) -> "Fixed":
        return Fixed(float(base) ** float(self))

    def fmod(self, other: Any) -> "Fixed":
        """Remainder with the sign of the dividend, same as the C# % operator."""
        o = Fixed(other)
        if o._raw == 0:
            return Fixed.ZERO
        r = abs(self._raw) % abs(o._raw)
        return Fixed.from_raw(r if self._raw >= 0 else -r)

    def sqrt(self) -> "Fixed":
        """Square root, bit exact with the C# FixedMath.Sqrt (negative values return 0)."""
        if self._raw <= 0:
            return Fixed.ZERO
        n = self._raw << FRACTIONAL_BITS
        root = math.isqrt(n)
        if n - root * root > root:
            root += 1
        return Fixed.from_raw(root)

    # ------------------------------------------------------------------ comparisons

    def _cmp_raw(self, other: Any):
        o = Fixed._coerce(other)
        return None if o is None else o._raw

    def __eq__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw == o

    def __ne__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw != o

    def __lt__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw < o

    def __le__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw <= o

    def __gt__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw > o

    def __ge__(self, other: Any) -> bool:
        o = self._cmp_raw(other)
        return NotImplemented if o is None else self._raw >= o

    def __hash__(self) -> int:
        return hash(float(self))

    # ------------------------------------------------------------------ formatting

    def __str__(self) -> str:
        """Shortest decimal that converts back to exactly this value (same as the C# ToString)."""
        raw = self._raw
        negative = raw < 0
        a = -raw if negative else raw
        integer_part = a >> FRACTIONAL_BITS
        fraction = a & _FRACTION_MASK
        sign = "-" if negative else ""
        if fraction == 0:
            return f"{sign}{integer_part}"
        pow10 = 1
        for digits in range(1, 7):
            pow10 *= 10
            candidate = _rounded_divide(fraction * pow10, ONE_RAW)
            if _rounded_divide(candidate * ONE_RAW, pow10) == fraction or digits == 6:
                if candidate == pow10:
                    integer_part += 1
                    candidate = 0
                decimals = str(candidate).rjust(digits, "0").rstrip("0")
                return f"{sign}{integer_part}.{decimals}" if decimals else f"{sign}{integer_part}"
        return repr(float(self))  # unreachable

    def __repr__(self) -> str:
        return f"Fixed({self})"

    def __format__(self, spec: str) -> str:
        return str(self) if not spec else format(float(self), spec)

    # ------------------------------------------------------------------ copy / pickle (immutable)

    def __reduce__(self):
        return (Fixed.from_raw, (self._raw,))

    def __copy__(self):
        return self

    def __deepcopy__(self, memo):
        return self

    # ------------------------------------------------------------------ pydantic integration

    @classmethod
    def __get_pydantic_core_schema__(cls, source_type: Any, handler: Any):
        from pydantic_core import core_schema

        return core_schema.no_info_plain_validator_function(
            cls._pydantic_validate,
            serialization=core_schema.plain_serializer_function_ser_schema(
                cls._pydantic_serialize, when_used="json"
            ),
        )

    @classmethod
    def __get_pydantic_json_schema__(cls, core_schema: Any, handler: Any):
        return {"type": "number"}

    @classmethod
    def _pydantic_validate(cls, value: Any) -> "Fixed":
        if isinstance(value, Fixed):
            return value
        try:
            return Fixed(value)
        except TypeError as e:
            raise ValueError(str(e)) from None

    @staticmethod
    def _pydantic_serialize(value: "Fixed") -> float:
        # exact: every Fixed is exactly representable as a float, and the game parses it back to the same value
        return float(value)


Fixed.ZERO = Fixed.from_raw(0)
Fixed.ONE = Fixed.from_raw(ONE_RAW)
Fixed.HALF = Fixed.from_raw(_HALF_RAW)
Fixed.MAX = Fixed.from_raw(_RAW_MAX)
Fixed.MIN = Fixed.from_raw(_RAW_MIN)
Fixed.EPSILON = Fixed.from_raw(1)
Fixed.PI = Fixed.from_raw(205887)
Fixed.HALF_PI = Fixed.from_raw(102944)
Fixed.TWO_PI = Fixed.from_raw(2 * 205887)
