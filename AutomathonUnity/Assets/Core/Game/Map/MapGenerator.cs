using Automathon.Engine;

namespace Automathon.Game.MapSystem
{
    public static class MapGenerator
    {
        public static void InstantiateMap(Map map)
        {
            string s = "";
            foreach (Entity e in map.Elements)
            {
                GameplayManager.Instantiate(e);

                if (e is Wall wall)
                {
                    int posX = wall.Position.X;
                    int posY = wall.Position.Y;
                    int sizeX = wall.Size.X;
                    int sizeY = wall.Size.Y;
                    int rot = wall.RotationMilli;

                    s += $"Instantiate(new Wall(new Vector2Int({posX}, {posY}), new Vector2Int({sizeX}, {sizeY}), {rot}));\n";
                }
            }

            //ça marche pas ta merde sur des builds espèce de salopio
            //File.WriteAllText("./Assets/Maps/lastGeneratedMapData.txt", s);
        }
    }
}
