using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Button Preset", fileName = "UIButtonPreset")]
    public sealed class UIButtonPreset : UIPresetBase
    {
        [TabGroup("Settings")]
        public bool interactable = true;

        [TabGroup("Settings")]
        public float doubleClickInterval = 0.3f;

        [TabGroup("Settings")]
        public float longClickDuration = 0.6f;

        [TabGroup("Settings")]
        public float clickCooldown = 0.1f;

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

        [TabGroup("Settings")]
        [Tooltip("If enabled, this button never plays click SFX.")]
        public bool muteUISound;

        [TabGroup("Settings")]
        [ShowIf("@!muteUISound")]
        public AudioClip customClickSound;

        [TabGroup("Animations")]
        [HideLabel]
        public UISelectableAnimationProfile stateAnimations = new UISelectableAnimationProfile();

        [TabGroup("Behaviours")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        [TabGroup("Callbacks")]
        public UnityEvent onClick = new UnityEvent();

        public void ApplyTo(UIButton button, UIPresetApplyMask overrideMask = null)
        {
            if (button != null)
            {
                button.ApplyButtonPresetData(this, true);
            }
        }
    }
}
