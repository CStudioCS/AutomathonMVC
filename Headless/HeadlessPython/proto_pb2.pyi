from google.protobuf.internal import containers as _containers
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from collections.abc import Iterable as _Iterable
from typing import ClassVar as _ClassVar, Optional as _Optional

DESCRIPTOR: _descriptor.FileDescriptor

class Action(_message.Message):
    __slots__ = ("action",)
    ACTION_FIELD_NUMBER: _ClassVar[int]
    action: _containers.RepeatedScalarFieldContainer[float]
    def __init__(self, action: _Optional[_Iterable[float]] = ...) -> None: ...

class Empty(_message.Message):
    __slots__ = ()
    def __init__(self) -> None: ...

class StepResponse(_message.Message):
    __slots__ = ("observation", "done")
    OBSERVATION_FIELD_NUMBER: _ClassVar[int]
    DONE_FIELD_NUMBER: _ClassVar[int]
    observation: _containers.RepeatedScalarFieldContainer[float]
    done: bool
    def __init__(self, observation: _Optional[_Iterable[float]] = ..., done: _Optional[bool] = ...) -> None: ...
