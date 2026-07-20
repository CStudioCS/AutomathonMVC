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

                else if(e is InvisibleWall invisibleWall)
                {
                    int posX = invisibleWall.Position.X;
                    int posY = invisibleWall.Position.Y;
                    int sizeX = invisibleWall.Size.X;
                    int sizeY = invisibleWall.Size.Y;
                    int rot = invisibleWall.RotationMilli;

                    s += $"Instantiate(new Wall(new Vector2Int({posX}, {posY}), new Vector2Int({sizeX}, {sizeY}), {rot}));\n";
                }
            }

            //ça marche pas ta merde sur des builds espèce de salopio  Gros fils dep c toi tu sais pas comment marche ton propre projet tu avais hard codé la map par pitie ça marche très bien
            //File.WriteAllText("./Assets/Maps/lastGeneratedMapData.txt", s);
        }
    }
}
