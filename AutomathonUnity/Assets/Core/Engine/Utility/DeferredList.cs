using System;
using System.Collections.Generic;

namespace Automathon.Utility
{
    /// <summary>
    /// Adding or removing elements in this lists doesn't happen instantly, but only when Flushed is called
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeferredList<T>
    {
        private List<T> items = new();
        private List<T> pendingAdd = new();
        private List<T> pendingRemove = new();

        public void Add(T item) => pendingAdd.Add(item);
        public void Remove(T item) => pendingRemove.Add(item);
        public void ProcessChanges()
        {
            foreach (var item in pendingRemove) items.Remove(item);
            foreach (var item in pendingAdd) items.Add(item);
            pendingAdd.Clear();
            pendingRemove.Clear();
        }

        public void ProcessChanges(Action<T> onAdded, Action<T> onRemoved)
        {
            foreach (var item in pendingRemove)
            {
                onRemoved?.Invoke(item);
                items.Remove(item);
            }
            foreach (var item in pendingAdd)
            {
                items.Add(item);
                onAdded?.Invoke(item);
            }
            pendingAdd.Clear();
            pendingRemove.Clear();
        }

        public IReadOnlyList<T> Items => items;
    }
}
