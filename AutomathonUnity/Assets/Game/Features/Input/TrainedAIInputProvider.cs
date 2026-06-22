using Automathon;
using Automathon.Game.Input;
using System;

namespace Core
{
    public class TrainedAIInputProvider : IInputProvider
    {
        //TODO: Make this use the trained AI ONNX with Unity Sentis to play the game
        public Vector2Int GetMilliAimingDir()
        {
            throw new NotImplementedException();
        }

        public Vector2Int GetMilliMovementDir()
        {
            throw new NotImplementedException();
        }

        public bool ShouldDash()
        {
            throw new NotImplementedException();
        }

        public bool ShouldMissile()
        {
            throw new NotImplementedException();
        }

        public bool ShouldShield()
        {
            throw new NotImplementedException();
        }

        public bool ShouldShoot()
        {
            throw new NotImplementedException();
        }
    }
}
