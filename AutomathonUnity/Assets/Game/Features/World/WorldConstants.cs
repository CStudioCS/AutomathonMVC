namespace Automathon.Game
{
    public static class WorldConstants
    {
        /// <summary>
        /// Unity units per simulation unit. The simulation (Core) works with Fixed numbers where 1 = 1 Unity unit,
        /// so positions/sizes only need to be converted from Fixed to float.
        /// </summary>
        public const float SPACE_SCALE = 1f;
    }
}
