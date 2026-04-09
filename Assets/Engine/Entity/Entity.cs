using Automathon.Utility;

namespace Automathon.Engine
{
    public abstract class Entity
    {
        public Vector2Int Position;
        private DeferredList<Component> components = new();

        public Entity(Vector2Int position)
        {
            Position = position;
        }

        public virtual void Start() { }

        /// <summary>
        /// Instanties/Destroys components that need to be
        /// </summary>
        public void ApplyComponentsChanges()
        {
            components.ProcessChanges();
        }

        public virtual void Update()
        {
            foreach (Component component in components.Items)
                component.Update();
        }

        public virtual void OnDestroyed()
        {

        }
    }
}
