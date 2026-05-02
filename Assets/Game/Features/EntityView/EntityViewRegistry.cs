using Automathon.Engine;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Automathon.Game.View.Registry
{
    //Written by Claude
    [CreateAssetMenu(menuName = "Game/Entity View Registry")]
    public class EntityViewRegistry : ScriptableObject
    {
        [SerializeField] private List<EntityView> prefabs = new();

        private Dictionary<Type, EntityView> cache;

#if UNITY_EDITOR
        private void OnValidate()
        {
            var seen = new HashSet<Type>();
            for (int i = prefabs.Count - 1; i >= 0; i--)
            {
                // Skip nulls (empty slots in the list)
                if (prefabs[i] == null) continue;

                var type = prefabs[i].EntityType;
                if (!seen.Add(type))
                {
                    UnityEngine.Debug.LogWarning($"[EntityViewRegistry] Duplicate for {type.Name} was removed.");
                    prefabs.RemoveAt(i);
                }
            }

            cache = null; // Invalidate cache whenever the list changes
        }
#endif

        public Dictionary<Type, EntityView> GetDictionary()
        {
            if (cache != null) return cache;

            cache = new Dictionary<Type, EntityView>();
            foreach (var prefab in prefabs)
            {
                var type = prefab.EntityType;
                if (!cache.TryAdd(type, prefab))
                    UnityEngine.Debug.LogWarning($"[EntityViewRegistry] Duplicate mapping for {type.Name}");
            }
            return cache;
        }

        public EntityView GetPrefabFor<T>() where T : Entity
            => GetDictionary().GetValueOrDefault(typeof(T));

        public EntityView GetPrefabFor(Type entityType)
            => GetDictionary().GetValueOrDefault(entityType);
    }
}
