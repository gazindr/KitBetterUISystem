using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Slider Preset", fileName = "UISliderPreset")]
    public sealed class UISliderPreset : UIPresetBase
    {
        [TabGroup("Settings")]
        public bool interactable = true;

        [TabGroup("Settings")]
        public float minValue;

        [TabGroup("Settings")]
        public float maxValue = 1f;

        [TabGroup("Settings")]
        public float value = 0.5f;

        [TabGroup("Settings")]
        public bool wholeNumbers;

        [TabGroup("Settings")]
        public Slider.Direction direction = Slider.Direction.LeftToRight;

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
        public RectTransform fillTarget;

        [TabGroup("Targets")]
        public RectTransform handleTarget;

        [TabGroup("Animations")]
        [HideLabel]
        public UISelectableAnimationProfile stateAnimations = new UISelectableAnimationProfile();

        [TabGroup("Behaviours")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        [TabGroup("Callbacks")]
        public UIFloatEvent onValueChanged = new UIFloatEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSliderDown = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSliderUp = new UnityEvent();

        public void ApplyTo(UISlider slider, UIPresetApplyMask overrideMask = null)
        {
            if (slider != null)
            {
                slider.ApplySliderPresetData(this, ResolveMask(overrideMask));
            }
        }
    }
}



