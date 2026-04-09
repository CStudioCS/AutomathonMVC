using Automathon.Utility;
using System.Linq;

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
            components.ProcessChanges((component) => component.Start(), (component) => component.OnRemoved());
        }

        public virtual void Update()
        {
            foreach (Component component in components.Items)
                component.Update();
        }

        public virtual void OnDestroyed()
        {

        }

        public T AddComponent<T>(T component) where T : Component
        {
            components.Add(component);
            return component;
        }

        public void RemoveComponent<T>(T component) where T : Component
        {
            components.Remove(component);
        }

        public bool TryGetComponent<T>(out Component component) where T : Component
        {
            component = null;
            foreach(Component c in components.Items)
            {
                if (c is T t)
                {
                    component = t;
                    return true;
                }
            }

            return false;
        }
    }
}
