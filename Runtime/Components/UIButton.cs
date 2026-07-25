using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Project.UI
{
    [AddComponentMenu("UI System/UIButton")]
    public sealed class UIButton : UISelectable
    {
        [TabGroup("Settings")]
        [Min(0f)]
        public float clickCooldown = 0.1f;

        [TabGroup("Settings")]
        [Tooltip("If enabled, this button never plays click SFX.")]
        public bool muteUISound;

        [TabGroup("Settings")]
        [ShowIf("@!muteUISound")]
        [Tooltip("Optional clip that overrides the global UI click sound from SFXManager.")]
        public AudioClip customClickSound;

        [TabGroup("Callbacks")]
        public UnityEvent onClick = new UnityEvent();

        [TabGroup("Presets")]
        public UIButtonPreset preset;

        [SerializeField]
        private List<string> overriddenPaths = new List<string>();

        private float lastClickTime = -999999f;

        public UIButtonPreset Preset
        {
            get { return preset; }
            set { preset = value; }
        }

        public List<string> OverriddenPaths
        {
            get { return overriddenPaths; }
        }

        protected override void Reset()
        {
            base.Reset();
            stateAnimations = UIAnimationDefaults.CreateButtonProfile();
            overriddenPaths = new List<string>();

            UIButtonPreset defaultPreset = null;
            UISystemDefaults defaults = UISystemDefaults.Instance;
            if (defaults != null)
            {
                defaultPreset = defaults.defaultButtonPreset;
            }

            if (defaultPreset == null)
            {
                defaultPreset = Resources.Load<UIButtonPreset>("Default-UIButtonPreset");
            }

            if (defaultPreset != null)
            {
                preset = defaultPreset;
                ApplyButtonPresetData(defaultPreset, true);
            }
        }

        public void Click()
        {
            if (!IsInteractable() && blockPointerWhenDisabled)
            {
                return;
            }

            if (!CanAcceptClick())
            {
                return;
            }

            MarkClickAccepted();
            PlayClickSound();

            if (onClick != null)
            {
                onClick.Invoke();
            }

            ExecuteTrigger(UIBehaviourTrigger.PointerLeftClick);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable() && blockPointerWhenDisabled)
            {
                return;
            }

            if (eventData == null || eventData.button == PointerEventData.InputButton.Left)
            {
                if (!CanAcceptClick())
                {
                    return;
                }

                MarkClickAccepted();
                PlayClickSound();
            }

            if (eventData == null || eventData.button == PointerEventData.InputButton.Left)
            {
                if (onClick != null)
                {
                    onClick.Invoke();
                }
            }

            base.OnPointerClick(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (!invokeOnSubmit)
            {
                return;
            }

            if (!IsInteractable() && blockPointerWhenDisabled)
            {
                return;
            }

            if (!CanAcceptClick())
            {
                return;
            }

            MarkClickAccepted();
            PlayClickSound();
            base.OnSubmit(eventData);
            if (onClick != null)
            {
                onClick.Invoke();
            }
        }

        public void ApplyButtonPresetData(UIButtonPreset sourcePreset)
        {
            ApplyButtonPresetData(sourcePreset, true);
        }

        public void ApplyButtonPresetData(UIButtonPreset sourcePreset, bool clearOverrides)
        {
            if (sourcePreset == null)
            {
                return;
            }

            interactable = sourcePreset.interactable;
            doubleClickInterval = sourcePreset.doubleClickInterval;
            longClickDuration = sourcePreset.longClickDuration;
            clickCooldown = sourcePreset.clickCooldown;
            blockPointerWhenDisabled = sourcePreset.blockPointerWhenDisabled;
            invokeOnSubmit = sourcePreset.invokeOnSubmit;
            useInQueue = sourcePreset.useInQueue;
            queueGroup = sourcePreset.queueGroup;
            queueReleaseDelay = sourcePreset.queueReleaseDelay;
            muteUISound = sourcePreset.muteUISound;
            customClickSound = sourcePreset.customClickSound;

            if (stateAnimations == null)
            {
                stateAnimations = new UISelectableAnimationProfile();
            }

            stateAnimations.CopyFrom(sourcePreset.stateAnimations);

            if (sourcePreset.behaviours != null)
            {
                behaviours = CloneBehaviourBlocks(sourcePreset.behaviours, false);
            }

            if (clearOverrides)
            {
                UIPresetOverrideUtility.ClearOverrides(overriddenPaths);
            }
        }

        public void ApplyPresetKeepingOverrides()
        {
            if (preset == null)
            {
                return;
            }

            if (overriddenPaths == null || overriddenPaths.Count == 0)
            {
                ApplyButtonPresetData(preset, false);
                return;
            }

#if UNITY_EDITOR
            UIPresetOverrideSync.ApplyNonOverridden(this, preset, overriddenPaths);
#else
            ApplyButtonPresetData(preset, false);
#endif
        }

        public bool IsPathOverridden(string path)
        {
            return UIPresetOverrideUtility.IsOverridden(overriddenPaths, path);
        }

        public void SetPathOverridden(string path, bool isOverride)
        {
            UIPresetOverrideUtility.SetOverride(overriddenPaths, path, isOverride);
        }

        public void ApplyPresetFromInspector()
        {
            if (preset != null)
            {
                ApplyButtonPresetData(preset, true);
            }
        }

        public void SaveAllToPreset()
        {
            if (preset == null)
            {
                return;
            }

            preset.interactable = interactable;
            preset.doubleClickInterval = doubleClickInterval;
            preset.longClickDuration = longClickDuration;
            preset.clickCooldown = clickCooldown;
            preset.blockPointerWhenDisabled = blockPointerWhenDisabled;
            preset.invokeOnSubmit = invokeOnSubmit;
            preset.useInQueue = useInQueue;
            preset.queueGroup = queueGroup;
            preset.queueReleaseDelay = queueReleaseDelay;
            preset.muteUISound = muteUISound;
            preset.customClickSound = customClickSound;

            if (preset.stateAnimations == null)
            {
                preset.stateAnimations = new UISelectableAnimationProfile();
            }

            preset.stateAnimations.CopyFrom(stateAnimations);

            if (behaviours != null)
            {
                preset.behaviours = CloneBehaviourBlocks(behaviours, false);
            }

            UIPresetOverrideUtility.ClearOverrides(overriddenPaths);
        }

        private bool CanAcceptClick()
        {
            return clickCooldown <= 0f || Time.unscaledTime - lastClickTime >= clickCooldown;
        }

        private void MarkClickAccepted()
        {
            lastClickTime = Time.unscaledTime;
        }

        private void PlayClickSound()
        {
            UISFX.Play(UISFXKind.Click, customClickSound, muteUISound);
        }
    }
}
