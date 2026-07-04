namespace Automathon.Engine
{
    public abstract class Behavior
    {

        public Entity ParentEntity { get; private set; }
        public Behavior()
        {
        }

        public void Initialize(Entity parentEntity)
        {
            ParentEntity = parentEntity;
        }

        public virtual void Start() { }
        public virtual void OnRemoved() { }

        public virtual void Update() { }
    }
}
