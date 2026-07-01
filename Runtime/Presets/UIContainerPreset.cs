using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Container Preset", fileName = "UIContainerPreset")]
    public sealed class UIContainerPreset : UIPresetBase
    {
        [TabGroup("Settings")]
        public string category;

        [TabGroup("Settings")]
        public bool autoRegister = true;

        [TabGroup("Settings")]
        public bool registerOnAwake = true;

        [TabGroup("Settings")]
        public UIContainerStartupMode startupMode = UIContainerStartupMode.InstantHide;

        [TabGroup("Settings")]
        public bool useInQueue;

        [TabGroup("Settings")]
        [ShowIf(nameof(useInQueue))]
        public string queueGroup = "Default";

        [TabGroup("Settings")]
        [ShowIf(nameof(useInQueue))]
        [MinValue(0f)]
        public float queueShowDelay;

        [TabGroup("Settings")]
        public bool useAutoHide;

        [TabGroup("Settings")]
        [ShowIf(nameof(useAutoHide))]
        public float autoHideDelay = 1f;

        [TabGroup("Animations")]
        [HideLabel]
        public UIContainerAnimationProfile animations = new UIContainerAnimationProfile();

        [TabGroup("Background")]
        [HideLabel]
        public UIBackgroundSettings backgroundSettings = new UIBackgroundSettings();

        [TabGroup("Callbacks")]
        public UnityEvent onShow = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onVisible = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onHide = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onHidden = new UnityEvent();

        [TabGroup("Callbacks")]
        public UIBoolEvent visibilityChanged = new UIBoolEvent();

        public void ApplyTo(UIContainer container, UIPresetApplyMask overrideMask = null)
        {
            if (container != null)
            {
                container.ApplyContainerPresetData(this, ResolveMask(overrideMask));
            }
        }

        private void OnValidate()
        {
            queueShowDelay = Mathf.Max(0f, queueShowDelay);
            autoHideDelay = Mathf.Max(0f, autoHideDelay);
        }
    }
}



