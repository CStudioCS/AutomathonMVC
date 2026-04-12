namespace Automathon.Engine
{
    public abstract class Component
    {
        //this is set by the entity on AddComponent,
        //so that there is no need to pass it as an argument to every component's constructor
        //So it won't be set in the components constructor, but it will be set before Start is called
        public Entity ParentEntity; 
        public Component()
        {
        }

        public virtual void Start() { }
        public virtual void OnRemoved() { }

        public virtual void Update() { }
    }
}
