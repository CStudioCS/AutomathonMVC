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
        public abstract void Initialize(Entity entity);
    }

    public class EntityView<T> : EntityView where T : Entity
    {
        public T Entity { get; private set; }
        public override Type EntityType => typeof(T);

        public sealed override void Initialize(Entity entity) => Initialize((T)entity);
        public virtual void Initialize(T entity)
        {
            Entity = entity;
            entity.Destroyed += OnControllerDestroyed;
        }

        protected virtual void LateUpdate()
        {
            transform.SetPositionAndRotation(Entity.Position.ToVector2Scaled(), ViewMath.MilliRadRotationToQuaternion(Entity.RotationMilli));
        }

        protected virtual void OnControllerDestroyed()
        {
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (Entity != null)
                Entity.Destroyed -= OnControllerDestroyed;
        }
    }
}

