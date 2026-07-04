namespace Automathon.Engine
{
    public abstract class Component : IUpdatable
    {
        //this has a lot of overlap with Behavior, will maybe add a base class that encapsulates both, will see later
        public Entity ParentEntity { get; private set; }

        public virtual void Initialize(Entity parentEntity)
        {
            ParentEntity = parentEntity;
        }

        public virtual void Update() { }

        public virtual void OnDestroyed() { }
    }
}
