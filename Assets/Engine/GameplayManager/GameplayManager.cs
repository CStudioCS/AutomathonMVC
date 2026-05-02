using Automathon.Engine.Physics;
using Automathon.Utility;

namespace Automathon.Engine
{
    public static class GameplayManager
    {
        private static DeferredList<Entity> entities = new();

        public static void Initialize()
        {
            PhysicsManager.Initialize();
        }

        public static void Update()
        {
            EntityUpdateLoop();

            entities.ProcessChanges((e) => e.Start(), (e) => e.Destroyed()); //Instanciate/Destroy every entity that needs to be

            foreach (Entity entity in entities.Items) //Add/Remove new entity components
                entity.ApplyComponentsChanges();

            PhysicsManager.Step();
        }

        public static void EntityUpdateLoop()
        {
            foreach (Entity entity in entities.Items)
                entity.Update();
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
            PhysicsManager.Dispose();
        }
    }
}