using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Project.UI
{
    [AddComponentMenu("UI System/UIToggle")]
    public sealed class UIToggle : UISelectable
    {
        [TabGroup("Settings")]
        [SerializeField]
        private bool isOn;

        [TabGroup("Settings")]
        [MinValue(0)]
        public int multipleSelectCount;

        [TabGroup("Settings")]
        public bool resetMultipleCounterOnDeselect = true;

        [TabGroup("Settings")]
        [ReadOnly]
        [ShowInInspector]
        public int SelectCount
        {
            get { return selectCount; }
        }

        [TabGroup("States")]
        public RectTransform backgroundTarget;

        [TabGroup("States")]
        public RectTransform handleTarget;

        [TabGroup("States")]
        [FoldoutGroup("States/Background Select")]
        [HideLabel]
        public UIAnimationState backgroundSelectAnimation = new UIAnimationState();

        [TabGroup("States")]
        [FoldoutGroup("States/Background Deselect")]
        [HideLabel]
        public UIAnimationState backgroundDeselectAnimation = new UIAnimationState();

        [TabGroup("States")]
        [FoldoutGroup("States/Handle Select")]
        [HideLabel]
        public UIAnimationState handleSelectAnimation = new UIAnimationState();

        [TabGroup("States")]
        [FoldoutGroup("States/Handle Deselect")]
        [HideLabel]
        public UIAnimationState handleDeselectAnimation = new UIAnimationState();

        [TabGroup("Callbacks")]
        public UIBoolEvent onValueChanged = new UIBoolEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSelected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onDeselected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onMultipleSelect = new UnityEvent();

        [TabGroup("Presets")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UITogglePreset preset;

        [TabGroup("Presets")]
        [HideLabel]
        public UIPresetApplyMask presetApplyMask = new UIPresetApplyMask();

        [SerializeField]
        [HideInInspector]
        private int selectCount;

        public bool IsOn
        {
            get { return isOn; }
        }

        /// <summary>Doozy-compatible event alias.</summary>
        public UIBoolEvent OnValueChangedCallback
        {
            get { return onValueChanged; }
        }

        public new void Select()
        {
            SetIsOn(true, false, true);
        }

        public void Select(bool instant)
        {
            SetIsOn(true, instant, true);
        }

        public void Deselect(bool instant = false)
        {
            SetIsOn(false, instant, true);
        }

        public void Toggle(bool instant = false)
        {
            SetIsOn(!isOn, instant, true);
        }

        public void SetIsOn(bool value, bool instant = false, bool invokeCallbacks = true)
        {
            bool changed = isOn != value;
            isOn = value;

            SetState(isOn ? UISelectableState.Selected : UISelectableState.Normal, instant);
            PlayToggleAnimations(isOn, instant);

            if (!invokeCallbacks)
            {
                return;
            }

            if (changed && onValueChanged != null)
            {
                onValueChanged.Invoke(isOn);
            }

            if (isOn)
            {
                selectCount++;
                if (onSelected != null)
                {
                    onSelected.Invoke();
                }

                ExecuteTrigger(UIBehaviourTrigger.Selected, null, true, UISelectableState.Selected);

                if (multipleSelectCount > 0 && selectCount % multipleSelectCount == 0)
                {
                    if (onMultipleSelect != null)
                    {
                        onMultipleSelect.Invoke();
                    }

                    ExecuteTrigger(UIBehaviourTrigger.MultipleSelect, null, true, UISelectableState.Selected);
                }
            }
            else if (changed)
            {
                if (resetMultipleCounterOnDeselect)
                {
                    selectCount = 0;
                }

                if (onDeselected != null)
                {
                    onDeselected.Invoke();
                }

                ExecuteTrigger(UIBehaviourTrigger.Deselected, null, true, UISelectableState.Normal);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (!IsInteractable() && blockPointerWhenDisabled)
            {
                return;
            }

            if (eventData == null || eventData.button == PointerEventData.InputButton.Left)
            {
                Toggle(false);
            }
        }

        public void ApplyTogglePresetData(UITogglePreset sourcePreset, UIPresetApplyMask mask)
        {
            if (sourcePreset == null)
            {
                return;
            }

            ApplySelectablePresetData(sourcePreset.stateAnimations, sourcePreset.behaviours, mask);

            if (mask == null || mask.ShouldApplySettings)
            {
                interactable = sourcePreset.interactable;
                multipleSelectCount = sourcePreset.multipleSelectCount;
                resetMultipleCounterOnDeselect = sourcePreset.resetMultipleCounterOnDeselect;
                blockPointerWhenDisabled = sourcePreset.blockPointerWhenDisabled;
                invokeOnSubmit = sourcePreset.invokeOnSubmit;
                useInQueue = sourcePreset.useInQueue;
                queueGroup = sourcePreset.queueGroup;
                queueReleaseDelay = sourcePreset.queueReleaseDelay;
            }

            if (mask != null && mask.ShouldApplyTargets)
            {
                backgroundTarget = sourcePreset.backgroundTarget;
                handleTarget = sourcePreset.handleTarget;
            }

            if (mask == null || mask.ShouldApplyAnimations)
            {
                backgroundSelectAnimation.CopyFrom(sourcePreset.backgroundSelectAnimation);
                backgroundDeselectAnimation.CopyFrom(sourcePreset.backgroundDeselectAnimation);
                handleSelectAnimation.CopyFrom(sourcePreset.handleSelectAnimation);
                handleDeselectAnimation.CopyFrom(sourcePreset.handleDeselectAnimation);
            }

            if (mask != null && mask.ShouldApplyCallbacks)
            {
                onValueChanged = sourcePreset.onValueChanged;
                onSelected = sourcePreset.onSelected;
                onDeselected = sourcePreset.onDeselected;
                onMultipleSelect = sourcePreset.onMultipleSelect;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetIsOn(isOn, true, false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            backgroundSelectAnimation.EnsureTypes();
            backgroundDeselectAnimation.EnsureTypes();
            handleSelectAnimation.EnsureTypes();
            handleDeselectAnimation.EnsureTypes();
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

        private void PlayToggleAnimations(bool selected, bool instant)
        {
            UIAnimationRunner.PlayState(this, backgroundTarget, selected ? backgroundSelectAnimation : backgroundDeselectAnimation, instant, null);
            UIAnimationRunner.PlayState(this, handleTarget, selected ? handleSelectAnimation : handleDeselectAnimation, instant, null);
        }

#if UNITY_EDITOR
        public void EditorPreviewSelect()
        {
            EditorPreviewState(UISelectableState.Selected);
            UIEditorAnimationPreview.PlayState(this, backgroundTarget, backgroundSelectAnimation, null);
            UIEditorAnimationPreview.PlayState(this, handleTarget, handleSelectAnimation, null);
        }

        public void EditorPreviewDeselect()
        {
            EditorPreviewState(UISelectableState.Normal);
            UIEditorAnimationPreview.PlayState(this, backgroundTarget, backgroundDeselectAnimation, null);
            UIEditorAnimationPreview.PlayState(this, handleTarget, handleDeselectAnimation, null);
        }
#endif
    }
}



