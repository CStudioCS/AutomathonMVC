using Automathon.Engine;
using System;
using System.Collections.Generic;

namespace Automathon.Game.MapSystem
{
    public class RandomMapGenerator
    {
        private int mapLength = 27000;
        private int mapHeight = 15000;

        private int definitionX = 10;
        private int definitionY => mapHeight * definitionX / mapLength;

        private int lengthX;
        private int lengthY;

        Random random;

        Vector2Int spawnA = new Vector2Int(-6750,0);
        Vector2Int spawnB = new Vector2Int(6750, 0);

        public static void GenerateRandomMap()
        {
            RandomMapGenerator gen = new();

            List<Entity> wallArray =
                new List<Entity>(gen.GenerateWallList());

            MapGenerator.InstantiateMap(
                new Map("random_map", wallArray));
        }

        //Generation des murs
        private List<Wall> GenerateWallList()
        {
            bool[,] walls = WallPlacement();
            List<Wall> wallList = new List<Wall>();

            lengthX = mapLength / definitionX;
            lengthY = mapHeight / definitionY;

            AddWalls(wallList, walls);
            return wallList;
        }
        private void AddWalls(List<Wall> wallList, bool[,] walls)
        {
            for(int j = 0; j < definitionX*definitionY; j++)
            {
                AddWall(wallList, walls, j);
            }
        }
        private void AddWall(List<Wall> wallList, bool[,] walls, int j)
        {
            int origineX = -mapLength / 2;
            int origineY = -mapHeight / 2;

            if (j % definitionX + 1 < definitionX)
            {
                if (walls[j,j+1])
                {
                    int posX = origineX + (j % definitionX + 1) * lengthX;
                    int posY = origineY + (j / definitionX) * lengthY + lengthY / 2;

                    wallList.Add(new Wall(new Vector2Int(posX, posY), new Vector2Int(lengthX / 10, lengthY), 0));
                }

                
            }
            if (j < definitionX * (definitionY - 1))
            {
                if (walls[j,j + definitionX])
                {
                    int posX = origineX + (j % definitionX) * lengthX + lengthX/2;
                    int posY = origineY + (j / definitionX + 1) * lengthY;

                    wallList.Add(new Wall(new Vector2Int(posX, posY), new Vector2Int(lengthX, lengthY/10), 0));
                }
            }
        }

        // -------------------- Génération du placement des murs sur la grille --------------------

        public bool[,] WallPlacement()
        {
            random = new Random();
            bool[,] walls = new bool[definitionX* definitionY, definitionX * definitionY];

            for (int i = 0; i < walls.GetLength(0); i++)
            {
                for (int j = 0; j < walls.GetLength(1); j++)
                {
                    walls[i, j] = true;
                }
            }

            Elaguage(walls);

            return walls;
        }

        private void Elaguage(bool[,] walls)
        {
            bool[] vue = new bool[definitionX * definitionY];
            TraiterNoeud(0, walls, vue);

            foreach (var (a, b) in PickRandomWalls(walls, 10))
            {
                walls[a, b] = false;
                walls[b, a] = false;
            }
        }

        private List<(int, int)> PickRandomWalls(bool[,] walls, int count)
        {
            List<(int, int)> murs = new List<(int, int)>();
            for (int j = 0; j < definitionX * definitionY; j++)
            {
                if (j % definitionX + 1 < definitionX && walls[j, j + 1])
                    murs.Add((j, j + 1));
                if (j < definitionX * (definitionY - 1) && walls[j, j + definitionX])
                    murs.Add((j, j + definitionX));
            }

            for (int k = 0; k < count && k < murs.Count; k++)
            {
                int r = random.Next(k, murs.Count);
                (murs[k], murs[r]) = (murs[r], murs[k]);
            }
            return murs.GetRange(0, Math.Min(count, murs.Count));
        }

        private void TraiterNoeud(int j, bool[,] walls, bool[] vue)
        {
            vue[j] = true;
            List<int> voisins = TrouverVoisinsNonVisites(j, vue);
            while (voisins.Count > 0)
            {
                int i = voisins[random.Next(voisins.Count)];
                walls[i, j] = false;
                walls[j, i] = false;
                TraiterNoeud(i, walls, vue);
                voisins = TrouverVoisinsNonVisites(j, vue);
            }
        }

        private List<int> TrouverVoisinsNonVisites(int j, bool[] vue)
        {
            List<int> voisins = new List<int>();
            if(j % definitionX > 0)
            {
                if (!vue[j - 1])
                {
                    voisins.Add(j - 1);
                }
            }
            if (j >= definitionX)
            {
                if (!vue[j - definitionX])
                {
                    voisins.Add(j - definitionX);
                }
            }
            if (j % definitionX + 1 < definitionX)
            {
                if (!vue[j + 1])
                {
                    voisins.Add(j + 1);
                }
            }
            if (j < definitionX * (definitionY - 1))
            {
                if (!vue[j + definitionX])
                {
                    voisins.Add(j + definitionX);
                }
            }
            return voisins;
        }



    }
}