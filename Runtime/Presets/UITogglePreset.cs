using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Toggle Preset", fileName = "UITogglePreset")]
    public sealed class UITogglePreset : UIPresetBase
    {
        [TabGroup("Settings")]
        public bool interactable = true;

        [TabGroup("Settings")]
        public int multipleSelectCount;

        [TabGroup("Settings")]
        public bool resetMultipleCounterOnDeselect = true;

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
        public RectTransform backgroundTarget;

        [TabGroup("Targets")]
        public RectTransform handleTarget;

        [TabGroup("Animations")]
        [HideLabel]
        public UISelectableAnimationProfile stateAnimations = new UISelectableAnimationProfile();

        [TabGroup("Animations")]
        [FoldoutGroup("Animations/Background Select")]
        [HideLabel]
        public UIAnimationState backgroundSelectAnimation = new UIAnimationState();

        [TabGroup("Animations")]
        [FoldoutGroup("Animations/Background Deselect")]
        [HideLabel]
        public UIAnimationState backgroundDeselectAnimation = new UIAnimationState();

        [TabGroup("Animations")]
        [FoldoutGroup("Animations/Handle Select")]
        [HideLabel]
        public UIAnimationState handleSelectAnimation = new UIAnimationState();

        [TabGroup("Animations")]
        [FoldoutGroup("Animations/Handle Deselect")]
        [HideLabel]
        public UIAnimationState handleDeselectAnimation = new UIAnimationState();

        [TabGroup("Behaviours")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        [TabGroup("Callbacks")]
        public UIBoolEvent onValueChanged = new UIBoolEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSelected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onDeselected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onMultipleSelect = new UnityEvent();

        public void ApplyTo(UIToggle toggle, UIPresetApplyMask overrideMask = null)
        {
            if (toggle != null)
            {
                toggle.ApplyTogglePresetData(this, ResolveMask(overrideMask));
            }
        }
    }
}



