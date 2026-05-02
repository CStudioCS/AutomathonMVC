using Automathon.Engine;
using Automathon.Game.Utility;
using Automathon.Utility;
using System;
using UnityEngine;

namespace Automathon.Game.View
{
    public abstract class EntityView : MonoBehaviour
    {
        public abstract Type EntityType { get; }
    }

    public class EntityView<T> : EntityView where T : Entity
    {
        public T Entity { get; private set; }
        public override Type EntityType => typeof(T);

        public virtual void Initialize(T entity)
        {
            Entity = entity;
        }

        protected virtual void Update()
        {
            transform.position = Entity.Position.ToVector2Scaled();
            transform.rotation = ViewMath.MilliRadRotationToQuaternion(Entity.RotationMilli);
        }
    }
}

