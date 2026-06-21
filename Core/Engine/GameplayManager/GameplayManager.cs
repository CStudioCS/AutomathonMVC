using Automathon.Engine.Physics;
using Automathon.Engine.Utility;
using Automathon.Game;
using Automathon.Utility;
using System;
using System.IO;

namespace Automathon.Engine
{
    public static class GameplayManager
    {
        private static DeferredList<Entity> entities = new();
        public static GameState State { get; private set; }
        public static event Action<Entity> EntitySpawned;

        public enum GameState { Lobby, Game }

        public static void Initialize()
        {
            State = GameState.Lobby;

            PhysicsManager.Initialize();
            PlayerManager.Initialize();

            Instantiate(new Wall(new Vector2Int(-5540, -1934), new Vector2Int(3975, 370), 3383));
            Instantiate(new Wall(new Vector2Int(-5612, -7016), new Vector2Int(5058, 401), 1721));
            Instantiate(new Wall(new Vector2Int(12536, 2478), new Vector2Int(4911, 417), 5639));
            Instantiate(new Wall(new Vector2Int(6988, 715), new Vector2Int(3831, 589), 2451));
            Instantiate(new Wall(new Vector2Int(1821, 5341), new Vector2Int(3729, 328), 5067));
            Instantiate(new Wall(new Vector2Int(-12551, 3547), new Vector2Int(5759, 226), 6022));
            Instantiate(new Wall(new Vector2Int(904, -236), new Vector2Int(4676, 482), 1242));
        }

        private static void GenerateRandomMap()
        {
            Random r = new();
            Debug.Log(Directory.GetCurrentDirectory());
            string s = "";
            for (int i = 0; i < 7; i++)
            {
                int big = r.Next(3000, 6000);
                int small = r.Next(200, 600);
                int rot = r.Next(0, IntMath.PI_MILLI * 2);
                int posX = r.Next(-13500, 13500);
                int posY = r.Next(-7500, 7500);

                Instantiate(new Wall(new Vector2Int(posX, posY), new Vector2Int(big, small), rot));
                s += $"Instantiate(new Wall(new Vector2Int({posX}, {posY}), new Vector2Int({big}, {small}), {rot}));\n";
            }

            File.WriteAllText("./Assets/Maps/lastGeneratedMapData.txt", s);
        }

        public static void Update()
        {
            EntityUpdateLoop();

            ProcessAllEntityChanges();

            PhysicsManager.Step();
        }

        public static float[] GetState()
        {
            return new float[0];
        }

        public static void EntityUpdateLoop()
        {
            foreach (Entity entity in entities.Items)
                entity.Update();
        }

        private static void ProcessAllEntityChanges()
        {
            entities.ProcessChanges((e) =>
            {
                EntitySpawned?.Invoke(e);
                e.Start();
            }, (e) => e.OnDestroyed()); //Instanciate/Destroy every entity that needs to be

            foreach (Entity entity in entities.Items) //Add/Remove new entity components
                entity.ApplyComponentsChanges();
        }

        public static T Instantiate<T>(T entity) where T : Entity
        {
            entities.Add(entity);
            return entity;
        }

        public static void Destroy(Entity entity)
        {
            entities.Remove(entity);
        }

        public static void ChangeState(GameState newState)
        {
            State = newState;
            //maybe emit an event here in the future
        }

        public static void Dispose()
        {
            PlayerManager.Dispose();

            foreach (Entity entity in entities.Items) Destroy(entity);

            ProcessAllEntityChanges();

            PhysicsManager.Dispose();
        }
    }
}