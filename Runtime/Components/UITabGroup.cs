using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [AddComponentMenu("UI System/UITab Group")]
    public sealed class UITabGroup : MonoBehaviour
    {
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UITab> tabs = new List<UITab>();

        public bool allowDeselectAll;

        [ReadOnly]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UITab selectedTab;

        public UnityEvent onSelectionChanged = new UnityEvent();

        public void Register(UITab tab)
        {
            if (tab == null)
            {
                return;
            }

            if (!tabs.Contains(tab))
            {
                tabs.Add(tab);
            }

            if (tab.IsSelected && selectedTab == null)
            {
                selectedTab = tab;
            }
        }

        public void Unregister(UITab tab)
        {
            if (tab == null)
            {
                return;
            }

            tabs.Remove(tab);
            if (selectedTab == tab)
            {
                selectedTab = null;
            }
        }

        public void SelectTab(UITab tab, bool instant = false)
        {
            if (tab == null)
            {
                if (allowDeselectAll)
                {
                    DeselectAll(instant);
                }

                return;
            }

            Register(tab);

            for (int i = 0; i < tabs.Count; i++)
            {
                UITab other = tabs[i];
                if (other != null && other != tab)
                {
                    other.SetSelected(false, instant, true);
                }
            }

            selectedTab = tab;
            tab.SetSelected(true, instant, true);
            if (onSelectionChanged != null)
            {
                onSelectionChanged.Invoke();
            }
        }

        [Button]
        public void DeselectAll(bool instant = false)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] != null)
                {
                    tabs[i].SetSelected(false, instant, true);
                }
            }

            selectedTab = null;
            if (onSelectionChanged != null)
            {
                onSelectionChanged.Invoke();
            }
        }
    }
}



