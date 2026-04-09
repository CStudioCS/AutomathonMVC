namespace Automathon.Engine
{
    public abstract class Component
    {
        public Entity ParentEntity;

        public Component(Entity parentEntity)
        {
            ParentEntity = parentEntity;
        }

        public virtual void Start() { }
        public virtual void OnRemoved() { }

        public virtual void Update() { }
    }
}
