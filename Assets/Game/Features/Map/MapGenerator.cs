using Automathon.Engine;
using System.IO;
using Newtonsoft.Json;

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

        public static void RegisterMap(Map map)
        {
            string json = JsonConvert.SerializeObject(
                map,
                Formatting.Indented
            );

            File.WriteAllText(map.Name + ".json", json);
        }
    } 
}
