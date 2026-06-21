using Automathon.Engine;

namespace Automathon.Game.MapSystem
{
    public static class MapGenerator
    {
        public static void InstantiateMap(Map map)
        {
            foreach (Entity e in map.Elements)
            {
                GameplayManager.Instantiate(e);
            }
        }
    }
}
