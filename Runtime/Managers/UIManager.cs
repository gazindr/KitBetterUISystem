using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [AddComponentMenu("UI System/UI Manager")]
    public sealed class UIManager : MonoBehaviour
    {
        public bool registerChildContainersOnAwake = true;

        private void Awake()
        {
            if (registerChildContainersOnAwake)
            {
                RegisterChildContainers();
            }
        }

        [Button]
        public void RegisterChildContainers()
        {
            UIContainer[] containers = GetComponentsInChildren<UIContainer>(true);
            for (int i = 0; i < containers.Length; i++)
            {
                UIRegistry.Register(containers[i]);
            }
        }

        public void Show(string id)
        {
            UIContainer.Show(id);
        }

        public void Hide(string id)
        {
            UIContainer.Hide(id);
        }

        public void Toggle(string id)
        {
            UIContainer.Toggle(id);
        }
    }
}



