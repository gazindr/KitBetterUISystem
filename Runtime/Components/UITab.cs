using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Project.UI
{
    [AddComponentMenu("UI System/UITab")]
    public sealed class UITab : UISelectable
    {
        [TabGroup("Settings")]
        [SerializeField]
        private bool isSelected;

        [TabGroup("Settings")]
        public UITabGroup group;

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

        [TabGroup("Settings")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UIContainer linkedContainer;

        [TabGroup("Settings")]
        [ShowIf(nameof(linkedContainer))]
        public bool showLinkedContainerOnSelect = true;

        [TabGroup("Settings")]
        [ShowIf(nameof(linkedContainer))]
        public bool hideLinkedContainerOnDeselect = true;

        [TabGroup("Callbacks")]
        public UnityEvent onSelected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onDeselected = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onMultipleSelect = new UnityEvent();

        [TabGroup("Presets")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UITabPreset preset;

        [TabGroup("Presets")]
        [HideLabel]
        public UIPresetApplyMask presetApplyMask = new UIPresetApplyMask();

        [SerializeField]
        [HideInInspector]
        private int selectCount;

        public bool IsSelected
        {
            get { return isSelected; }
        }

        public new void Select()
        {
            Select(false);
        }

        public void Select(bool instant)
        {
            if (group != null)
            {
                group.SelectTab(this, instant);
            }
            else
            {
                SetSelected(true, instant, true);
            }
        }

        public void Deselect(bool instant = false)
        {
            SetSelected(false, instant, true);
        }

        public void SetSelected(bool value, bool instant = false, bool invokeCallbacks = true)
        {
            bool changed = isSelected != value;
            isSelected = value;
            SetState(isSelected ? UISelectableState.Selected : UISelectableState.Normal, instant);

            if (isSelected && showLinkedContainerOnSelect && linkedContainer != null)
            {
                if (instant)
                {
                    linkedContainer.InstantShow();
                }
                else
                {
                    linkedContainer.Show();
                }
            }
            else if (!isSelected && hideLinkedContainerOnDeselect && linkedContainer != null)
            {
                if (instant)
                {
                    linkedContainer.InstantHide();
                }
                else
                {
                    linkedContainer.Hide();
                }
            }

            if (!invokeCallbacks)
            {
                return;
            }

            if (isSelected)
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
                Select(false);
            }
        }

        public void ApplyTabPresetData(UITabPreset sourcePreset, UIPresetApplyMask mask)
        {
            if (sourcePreset == null)
            {
                return;
            }

            ApplySelectablePresetData(sourcePreset.stateAnimations, null, mask);

            if (mask == null || mask.ShouldApplySettings)
            {
                interactable = sourcePreset.interactable;
                multipleSelectCount = sourcePreset.multipleSelectCount;
                resetMultipleCounterOnDeselect = sourcePreset.resetMultipleCounterOnDeselect;
                showLinkedContainerOnSelect = sourcePreset.showLinkedContainerOnSelect;
                hideLinkedContainerOnDeselect = sourcePreset.hideLinkedContainerOnDeselect;
                blockPointerWhenDisabled = sourcePreset.blockPointerWhenDisabled;
                invokeOnSubmit = sourcePreset.invokeOnSubmit;
                useInQueue = sourcePreset.useInQueue;
                queueGroup = sourcePreset.queueGroup;
                queueReleaseDelay = sourcePreset.queueReleaseDelay;
            }

            if (mask != null && mask.ShouldApplyTargets)
            {
                group = sourcePreset.group;
                linkedContainer = sourcePreset.linkedContainer;
            }

            if (mask != null && mask.ShouldApplyCallbacks)
            {
                onSelected = sourcePreset.onSelected;
                onDeselected = sourcePreset.onDeselected;
                onMultipleSelect = sourcePreset.onMultipleSelect;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (group != null)
            {
                group.Register(this);
            }

            SetSelected(isSelected, true, false);
        }

        protected override void OnDisable()
        {
            if (group != null)
            {
                group.Unregister(this);
            }

            base.OnDisable();
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

#if UNITY_EDITOR
        public void EditorPreviewSelect()
        {
            EditorPreviewState(UISelectableState.Selected);
        }

        public void EditorPreviewDeselect()
        {
            EditorPreviewState(UISelectableState.Normal);
        }
#endif
    }
}



