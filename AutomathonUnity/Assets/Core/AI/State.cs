using System.Collections.Generic;
using static Automathon.Game.Tank;

namespace Automathon.Game
{
    public class GameState
    {
        public bool Done;
        public TankState SelfTank;
        public TankState EnemyTank;
        public List<Bullet.BulletState> BulletStates = new();
        public List<Missile.MissileState> MissileStates = new();
        public List<Wall.WallState> WallStates = new();
        public List<Shield.ShieldState> ShieldStates = new();
    }

    public class State
    {
        public string Type => this.GetType().Name;
        /// <summary>World position (1 = 1 Unity unit).</summary>
        public FixedVector2 Position;
    }
}
