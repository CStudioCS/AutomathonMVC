using Automathon.Engine.Physics;
using Automathon.Utility;
using System;

namespace Automathon.Engine
{
    public static class GameplayManager
    {
        private static DeferredList<Entity> entities = new();

        public static event Action<Entity> EntitySpawned;

        public static void Initialize()
        {
            PhysicsManager.Initialize();
        }

        public static void Update()
        {
            EntityUpdateLoop();

            ProcessAllEntityChanges();

            PhysicsManager.Step();
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

        public static void Dispose()
        {
            foreach (Entity entity in entities.Items) Destroy(entity);

            ProcessAllEntityChanges();

            PhysicsManager.Dispose();
        }
    }
}