using Automathon.Utility;

namespace Automathon.Engine
{
    public class GameplayManager
    {
        private DeferredList<Entity> entities = new();

        public GameplayManager()
        {

        }

        public void Update()
        {
            entities.ProcessChanges((e) => e.Start(), (e) => e.OnDestroyed()); //Instanciate/Destroy everything that needs to be

            EntityUpdateLoop();

            PhysicsUpdate();
        }

        public void EntityUpdateLoop()
        {
            foreach (Entity entity in entities.Items)
                entity.ApplyComponentsChanges();

            foreach (Entity entity in entities.Items)
                entity.Update();
        }

        public void PhysicsUpdate()
        {
            //TODO: Implement physics update, using trygetcomponent<rigidbody>() on every entity
        }
    }
}