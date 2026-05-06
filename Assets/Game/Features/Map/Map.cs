using Automathon.Engine;
using Automathon.Game.MapSystem;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;

namespace Automathon.Game.MapSystem
{
    public class Map
    {
        public string Name;

        public List<Entity> Elements;

        public Map(string name, List<Entity> elements)
        {
            this.Name = name;
            Elements = elements;
        }

        public void AddElement(Entity entity)
        {
            Elements.Add(entity);
        }
    }
}

