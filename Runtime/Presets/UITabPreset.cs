using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Tab Preset", fileName = "UITabPreset")]
    public sealed class UITabPreset : UIPresetBase
    {
        [TabGroup("Settings")]
        public bool interactable = true;

        [TabGroup("Settings")]
        public int multipleSelectCount;

        [TabGroup("Settings")]
        public bool resetMultipleCounterOnDeselect = true;

        [TabGroup("Settings")]
        public bool showLinkedContainerOnSelect = true;

        [TabGroup("Settings")]
        public bool hideLinkedContainerOnDeselect = true;

        [TabGroup("Settings")]
        public bool blockPointerWhenDisabled = true;

        [TabGroup("Settings")]
        public bool invokeOnSubmit = true;

        [TabGroup("Settings")]
        public bool useInQueue;

        [TabGroup("Settings")]
        [ShowIf(nameof(useInQueue))]
        public string queueGroup = "Default";

        [TabGroup("Settings")]
        [ShowIf(nameof(useInQueue))]
        public float queueReleaseDelay = 0.05f;

        [TabGroup("Targets")]
        public UITabGroup group;

        [TabGroup("Targets")]
        public UIContainer linkedContainer;

        [TabGroup("Animations")]
        [HideLabel]
        public UISelectableAnimationProfile stateAnimations = new UISelectableAnimationProfile();

        [TabGroup("Behaviours")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        [TabGroup("Callbacks")]
        public UnityEvent onSelected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onDeselected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onMultipleSelect = new UnityEvent();

        public void ApplyTo(UITab tab, UIPresetApplyMask overrideMask = null)
        {
            if (tab != null)
            {
                tab.ApplyTabPresetData(this, ResolveMask(overrideMask));
            }
        }
    }
}



