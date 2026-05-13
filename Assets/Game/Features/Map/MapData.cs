using Automathon.Engine;
using Automathon.Game.WallSystem;
using System.Collections.Generic;

namespace Automathon.Game.MapSystem
{
    public class MapData
    {
        public string Name;
        public List<EntityData> ElementsData;


        public static MapData DataFromMap(Map map)
        {
            MapData data = new MapData
            {
                Name = map.Name,
                ElementsData = new List<EntityData>()
            };

            foreach (Entity e in map.Elements)
            {
                switch (e)
                {
                    case Wall wall:
                        data.ElementsData.Add(new WallData(wall.Position, new Vector2Int(wall.BoxCollider.Width / 2, wall.BoxCollider.Height / 2), wall.RotationMilli));
                        break;
                }
            }

            return data;
        }

        public static Map MapFromData(MapData data)
        {
            List<Entity> elements = new List<Entity>();

            foreach (EntityData e in data.ElementsData)
            {
                switch (e)
                {
                    case WallData wallData:
                        elements.Add(new Wall(wallData.Position, wallData.HalfSize, wallData.RotationMilli));
                        break;
                }
            }

            Map map = new Map(data.Name, elements);

            return map;
        }
    }

    public abstract class EntityData
    {
        public Vector2Int Position;
        public int RotationMilli;
        public EntityData(Vector2Int position, int rotationMilli)
        {
            Position = position;
            RotationMilli = rotationMilli;
        }
    }

    public class WallData : EntityData
    {
        public Vector2Int HalfSize;
        public WallData(Vector2Int position, Vector2Int halfSize, int rotationMilli) : base(position, rotationMilli)
        {
            HalfSize = halfSize;
        }
    }
}