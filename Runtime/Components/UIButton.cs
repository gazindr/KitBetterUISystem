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

        [TabGroup("Animations")]
        [Tooltip("Optional animation preset. Changes from the preset are marked with * until Save.")]
        public UIButtonAnimationPreset animationPreset;

        [TabGroup("Callbacks")]
        public UnityEvent onClick = new UnityEvent();

        [TabGroup("Presets")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UIButtonPreset preset;

        [TabGroup("Presets")]
        [HideLabel]
        public UIPresetApplyMask presetApplyMask = new UIPresetApplyMask();

        private float lastClickTime = -999999f;

        protected override void Reset()
        {
            base.Reset();
            stateAnimations = UIAnimationDefaults.CreateButtonProfile();
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

        public void ApplyButtonPresetData(UIButtonPreset sourcePreset, UIPresetApplyMask mask)
        {
            if (sourcePreset == null)
            {
                return;
            }

            ApplySelectablePresetData(sourcePreset.stateAnimations, sourcePreset.behaviours, mask);

            if (mask == null || mask.ShouldApplySettings)
            {
                interactable = sourcePreset.interactable;
                doubleClickInterval = sourcePreset.doubleClickInterval;
                longClickDuration = sourcePreset.longClickDuration;
                clickCooldown = sourcePreset.clickCooldown;
                blockPointerWhenDisabled = sourcePreset.blockPointerWhenDisabled;
                invokeOnSubmit = sourcePreset.invokeOnSubmit;
                useInQueue = sourcePreset.useInQueue;
                queueGroup = sourcePreset.queueGroup;
                queueReleaseDelay = sourcePreset.queueReleaseDelay;
            }

            if (mask != null && mask.ShouldApplyCallbacks)
            {
                onClick = sourcePreset.onClick;
            }
        }

        [TabGroup("Presets")]
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.7f, 1f)]
        private void ApplyPreset()
        {
            if (preset != null)
            {
                preset.ApplyTo(this, presetApplyMask);
            }
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



