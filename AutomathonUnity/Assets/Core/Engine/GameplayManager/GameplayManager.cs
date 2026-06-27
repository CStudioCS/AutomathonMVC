using Automathon.Engine.Physics;
using Automathon.Engine.Utility;
using Automathon.Game;
using Automathon.Game.Input;
using Automathon.Utility;
using NetMQ;
using System;
using System.IO;

namespace Automathon.Engine
{
    public static class GameplayManager
    {
        private static DeferredList<Entity> entities = new();
        public static GameplayState State { get; private set; }
        public static event Action<Entity> EntitySpawned;
        public static event Action<Tank.TeamType> GameEnded;

        public static Tank Tank1;
        public static Tank Tank2;

        public enum GameplayState { Lobby, Game }

        public static void Initialize()
        {
            State = GameplayState.Lobby;

            LayerMatrix.Initialize();
            PhysicsManager.Initialize();

            //Wait for players to log in
            //The View side / Headless side will handle calling Reset with the right input providers
            //We can't do like LPI where we would just let players play from the beginning, since there is AI we'll have to
            //have a menu to set it
            //Once reset is called, update can be managed from the other project using AIStep or simply Update

            //Last issue is -> if AI playing while not headless, we need to make sure to call the AI. Idk if we should do it from AIInputProvider
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

        public static void Reset(InputProvider inputProvider1, InputProvider inputProvider2)
        {
            foreach (Entity entity in entities.Items) Destroy(entity);

            ProcessAllEntityChanges();

            Instantiate(new Wall(new Vector2Int(-5540, -1934), new Vector2Int(3975, 370), 3383));
            Instantiate(new Wall(new Vector2Int(-5612, -7016), new Vector2Int(5058, 401), 1721));
            Instantiate(new Wall(new Vector2Int(12536, 2478), new Vector2Int(4911, 417), 5639));
            Instantiate(new Wall(new Vector2Int(6988, 715), new Vector2Int(3831, 589), 2451));
            Instantiate(new Wall(new Vector2Int(1821, 5341), new Vector2Int(3729, 328), 5067));
            Instantiate(new Wall(new Vector2Int(-12551, 3547), new Vector2Int(5759, 226), 6022));
            Instantiate(new Wall(new Vector2Int(904, -236), new Vector2Int(4676, 482), 1242));

            Tank1 = Instantiate(new Tank(Tank.TeamType.Green, new Vector2Int(-10000, 0), inputProvider1));
            Tank2 = Instantiate(new Tank(Tank.TeamType.Red, new Vector2Int(10000, 0), inputProvider2));

            ProcessAllEntityChanges();

            State = GameplayState.Game;
        }

        public static void EndGame(Tank.TeamType loser)
        {
            State = GameplayState.Lobby;

            Tank.TeamType winner = loser == Tank1.Team ? Tank2.Team : Tank1.Team;
            GameEnded?.Invoke(winner);
        }

        public static GameState GetState(InputProvider self)
        {
            GameState gameState = new GameState();
            int tankCount = 0;

            foreach (Entity entity in entities.Items)
            {
                if (entity is Tank t)
                {
                    tankCount++;
                    if (t.InputProvider == self)
                        gameState.SelfTank = (Tank.TankState)t.GetState();
                    else
                        gameState.EnemyTank = (Tank.TankState)t.GetState();
                }

                else if (entity is Bullet)
                    gameState.BulletStates.Add((Bullet.BulletState)entity.GetState());
                else if (entity is Missile)
                    gameState.MissileStates.Add((Missile.MissileState)entity.GetState());
                else if (entity is Wall)
                    gameState.WallStates.Add((Wall.WallState)entity.GetState());
                else if (entity is Shield)
                    gameState.ShieldStates.Add((Shield.ShieldState)entity.GetState());

            }

            gameState.Done = tankCount <= 1;

            return gameState;
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

        public static void ChangeState(GameplayState newState)
        {
            State = newState;
            //maybe emit an event here in the future
        }

        public static void Dispose()
        {
            foreach (Entity entity in entities.Items) Destroy(entity);

            ProcessAllEntityChanges();

            PhysicsManager.Dispose();

            NetMQConfig.Cleanup(false);
        }
    }
}