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
                    s += $"Instantiate(new Wall(new FixedVector2({wall.Position.X}, {wall.Position.Y}), new FixedVector2({wall.Size.X}, {wall.Size.Y}), {wall.Rotation}));\n";
                else if (e is InvisibleWall invisibleWall)
                    s += $"Instantiate(new InvisibleWall(new FixedVector2({invisibleWall.Position.X}, {invisibleWall.Position.Y}), new FixedVector2({invisibleWall.Size.X}, {invisibleWall.Size.Y}), {invisibleWall.Rotation}));\n";
            }

            //ça marche pas ta merde sur des builds espèce de salopio  Gros fils dep c toi tu sais pas comment marche ton propre projet tu avais hard codé la map par pitie ça marche très bien
            //File.WriteAllText("./Assets/Maps/lastGeneratedMapData.txt", s);
        }
    }
}
