namespace Automathon.AI
{
    public class AIAction
    {
        /// <summary>Movement direction. Vectors longer than 1 are normalized, shorter ones move the tank slower.</summary>
        public FixedVector2 MovingDirection;
        /// <summary>Aiming direction, normalized by the game. A zero vector keeps the previous aiming direction.</summary>
        public FixedVector2 AimingDirection;
        public bool MachineGun;
        public bool Missile;
        public bool Shield;
        public bool Dash;
    }
}
