using UnityEngine;

namespace Automathon.Game.Input
{
    public class EmptyInputProvider : IInputProvider
    {

        public bool Dash() => false;

        public bool Grenade() => false;

        public Vector2Int MilliMovementDir() => Vector2Int.Zero;

        public bool Shield() => false;

        public bool Shoot() => false;
    }
}