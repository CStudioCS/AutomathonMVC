When specifying X and Y, we're talking about vector directions. 

Action vector : 
0 - MovingDir.X
1 - MovingDir.Y (the moving vector should have a norm under or equal 1, else it will normalized to 1, also if sent as (0,0) then it won't change)
2 - AimingDir.X
3 - AimingDir.Y (aiming dir is normalized to 1, if set to (0, 0) it won't change)
4 - Shoot



TO REGENERATE PROTOBUF FILES :
python -m grpc_tools.protoc -I../.. --python_out=. --pyi_out=. --grpc_python_out=. proto.proto