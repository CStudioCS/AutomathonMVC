using Automathon.Engine.Physics;
using Automathon.Utility;
using System;

namespace Automathon.Engine
{
    public class GameplayManager : IDisposable
    {
        private DeferredList<Entity> entities = new();
        public enum GameState { Lobby, Game }
        private PhysicsManager physicsManager = new();
        public GameState State { get; private set; }
        public static GameplayManager Instance { get; private set; }

        public void Awake()
        {
            State = GameState.Lobby;
            if (Instance != null)
                return;
            Instance = this;
        }
        public void Update()
        {
            EntityUpdateLoop();

            entities.ProcessChanges((e) => e.Start(), (e) => e.OnDestroyed()); //Instanciate/Destroy every entity that needs to be

            foreach (Entity entity in entities.Items) //Add/Remove new entity components
                entity.ApplyComponentsChanges();

            physicsManager.Step();
        }

        public void EntityUpdateLoop()
        {
            foreach (Entity entity in entities.Items)
                entity.Update();
        }

        public T Instantiate<T>(T entity) where T : Entity
        {
            entities.Add(entity);
            return entity;
        }

        public void Destroy(Entity entity)
        {
            entities.Remove(entity);
        }

        public void Dispose()
        {
            physicsManager.Dispose();
        }
    }
}