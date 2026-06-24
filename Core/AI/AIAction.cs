namespace Automathon.AI
{
    public struct AIAction
    {
        public Vector2Int MovingDir;
        public Vector2Int AimingDir;
        public bool MachineGun;
        public bool Missile;
        public bool Shield;
        public bool Dash;
        public Vector2Int DashDir;

        public AIAction(string json)
        {

        }
    }
}
