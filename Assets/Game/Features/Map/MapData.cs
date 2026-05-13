using Automathon.Engine;
using System.Collections.Generic;

namespace Automathon.Game.MapSystem
{
    public class MapData
    {
        public string Name;
        public List<EntityData> ElementsData;

        public static MapData FromMap(Map map)
        {
            MapData data = new MapData
            {
                Name = map.Name,
                ElementsData = new List<EntityData>()
            };

            foreach (Entity e in map.Elements)
            {
                if (e is IPersistable persistable)
                    data.ElementsData.Add(persistable.ToData());
            }

            return data;
        }

        public Map ToMap()
        {
            List<Entity> elements = new List<Entity>();

            foreach (EntityData d in ElementsData)
            {
                if (d is IEntityFactory factory)
                    elements.Add(factory.ToEntity());
            }

            return new Map(Name, elements);
        }
    }
}
