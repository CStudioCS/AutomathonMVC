/*using Newtonsoft.Json;
using System.IO;
using UnityEngine;*/

namespace Automathon.Game.MapSystem
{
    public static class MapSaver
    {

        /*public static void RegisterMap(Map map)
        {
            string mapDirectory = Application.dataPath + "/Maps";

            Directory.CreateDirectory(mapDirectory);

            MapData data = MapData.FromMap(map);

            string json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );

            File.WriteAllText(
                mapDirectory + "/" + map.Name + ".json",
                json
            );
        }

        public static Map LoadMap(string mapName)
        {
            string path = Application.dataPath + "/Maps/" + mapName + ".json";

            if (!File.Exists(path))
            {
                Debug.LogError("Map file not found : " + path);
                return null;
            }

            string json = File.ReadAllText(path);

            MapData data = JsonConvert.DeserializeObject<MapData>(
                json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );

            return data.ToMap();
        }*/
    }
}
