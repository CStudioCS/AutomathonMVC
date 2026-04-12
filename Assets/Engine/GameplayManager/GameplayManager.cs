using Automathon.Engine.Physics;
using Automathon.Utility;
using System;

namespace Automathon.Engine
{
    public class GameplayManager : IDisposable
    {
        private DeferredList<Entity> entities = new();
        private PhysicsManager physicsManager;

        public GameplayManager()
        {
            physicsManager = new PhysicsManager();
        }

        public void Update()
        {
            entities.ProcessChanges((e) => e.Start(), (e) => e.OnDestroyed()); //Instanciate/Destroy everything that needs to be

            EntityUpdateLoop();

            physicsManager.Step();
        }

        public void EntityUpdateLoop()
        {
            foreach (Entity entity in entities.Items)
                entity.ApplyComponentsChanges();

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