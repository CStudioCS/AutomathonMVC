using Automathon.Utility;
using System;

namespace Automathon.Engine
{
    public abstract class Entity
    {
        public Vector2Int Position;
        public int RotationMilli;
        public event Action Destroyed;

        private Component[] components;
        private DeferredList<Behavior> behaviors = new();

        private bool initialized;

        public Entity(Vector2Int position)
        {
            Position = position;
        }

        protected void Initialize(params Component[] components)
        {
            if (initialized)
                throw new Exception("Cannot initialize an entity twice. This entity has already been initialized !");

            initialized = true;
            this.components = components;

            foreach (Component component in components)
                component.Initialize(this);
        }

        public virtual void Start()
        {
            if (!initialized)
                Initialize(new Component[] { });
        }

        /// <summary>
        /// Instanties/Destroys components that need to be
        /// </summary>
        public void ApplyComponentsChanges()
        {
            behaviors.ProcessChanges((behavior) =>
            {
                behavior.Initialize(this);
                behavior.Start();
            }, (component) => component.OnRemoved());
        }

        public virtual void Update()
        {
            foreach (Behavior behaviors in behaviors.Items)
                behaviors.Update();

            foreach (Component component in components)
                component.Update();
        }

        public virtual void OnDestroyed()
        {
            foreach (Behavior component in behaviors.Items)
                RemoveBehavior(component);

            foreach (Component component in components)
                component.OnDestroyed();

            Destroyed?.Invoke();
        }

        public T AddBehavior<T>(T behavior) where T : Behavior
        {
            behaviors.Add(behavior);
            return behavior;
        }

        public void RemoveBehavior<T>(T behavior) where T : Behavior
        {
            behaviors.Remove(behavior);
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            foreach (Component currentComponent in components)
            {
                if (currentComponent is T tComponent)
                {
                    component = tComponent;
                    return true;
                }
            }

            component = null;
            return false;
        }
    }
}
