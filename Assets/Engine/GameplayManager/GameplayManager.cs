using Automathon.Utility;

namespace Automathon.Engine
{
    public class GameplayManager
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
    }
}