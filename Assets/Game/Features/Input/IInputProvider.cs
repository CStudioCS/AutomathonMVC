using System;
using System.Collections.Generic;
using System.Text;

namespace Automathon.Game.Input
{
    public interface IInputProvider
    {
        public Vector2Int MilliMovementDir();
        public bool Shoot();
        public bool Shield();
        public bool Grenade();
        public bool Dash();
    }
}
