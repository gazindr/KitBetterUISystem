using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    public static class UIRegistry
    {
        private static readonly Dictionary<string, UIContainer> Containers = new Dictionary<string, UIContainer>();

        public static void Register(UIContainer container)
        {
            if (container == null)
            {
                return;
            }

            string id = container.Id;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[UISystem] UIContainer on " + container.name + " has empty id and cannot be registered.");
                return;
            }

            UIContainer existing;
            if (Containers.TryGetValue(id, out existing) && existing != null && existing != container)
            {
                Debug.LogWarning("[UISystem] Duplicate UIContainer id '" + id + "'. Replacing " + existing.name + " with " + container.name + ".");
            }

            Containers[id] = container;
        }

        public static void Unregister(UIContainer container)
        {
            if (container == null || string.IsNullOrEmpty(container.Id))
            {
                return;
            }

            UIContainer existing;
            if (Containers.TryGetValue(container.Id, out existing) && existing == container)
            {
                Containers.Remove(container.Id);
            }
        }

        public static bool TryGet(string id, out UIContainer container)
        {
            if (string.IsNullOrEmpty(id))
            {
                container = null;
                return false;
            }

            return Containers.TryGetValue(id, out container) && container != null;
        }

        public static UIContainer Get(string id)
        {
            UIContainer container;
            return TryGet(id, out container) ? container : null;
        }

        public static void Clear()
        {
            Containers.Clear();
        }

        public static List<UIContainer> GetAll()
        {
            List<UIContainer> result = new List<UIContainer>(Containers.Count);
            foreach (KeyValuePair<string, UIContainer> pair in Containers)
            {
                if (pair.Value != null)
                {
                    result.Add(pair.Value);
                }
            }

            return result;
        }
    }
}



