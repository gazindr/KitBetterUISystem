using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    [AddComponentMenu("UI System/UISlider")]
    public sealed class UISlider : UISelectable, IDragHandler, IInitializePotentialDragHandler
    {
        [TabGroup("Settings")]
        public float minValue;

        [TabGroup("Settings")]
        public float maxValue = 1f;

        [TabGroup("Settings")]
        [SerializeField]
        [LabelText("value")]
        private float sliderValue = 0.5f;

        [TabGroup("Settings")]
        public bool wholeNumbers;

        [TabGroup("Settings")]
        public Slider.Direction direction = Slider.Direction.LeftToRight;

        [TabGroup("Settings")]
        [ShowInInspector]
        public float normalizedValue
        {
            get { return Mathf.Approximately(maxValue, minValue) ? 0f : Mathf.InverseLerp(minValue, maxValue, sliderValue); }
            set { SetNormalizedValue(value, true); }
        }

        [TabGroup("States")]
        public RectTransform fillTarget;

        [TabGroup("States")]
        public RectTransform handleTarget;

        [TabGroup("Callbacks")]
        public UIFloatEvent onValueChanged = new UIFloatEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSliderDown = new UnityEvent();

        [TabGroup("Callbacks")]
        public UnityEvent onSliderUp = new UnityEvent();

        [TabGroup("Presets")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public UISliderPreset preset;

        [TabGroup("Presets")]
        [HideLabel]
        public UIPresetApplyMask presetApplyMask = new UIPresetApplyMask();

        public float Value
        {
            get { return sliderValue; }
            set { SetValue(value, true); }
        }

        public void SetValueWithoutNotify(float newValue)
        {
            SetValue(newValue, false);
        }

        public void SetValue(float newValue, bool sendCallback = true)
        {
            float clampedValue = ClampValue(newValue);
            if (Mathf.Approximately(sliderValue, clampedValue))
            {
                UpdateVisuals();
                return;
            }

            sliderValue = clampedValue;
            UpdateVisuals();

            if (sendCallback)
            {
                if (onValueChanged != null)
                {
                    onValueChanged.Invoke(sliderValue);
                }

                ExecuteTrigger(UIBehaviourTrigger.ValueChanged, null, false, UISelectableState.Normal, true, sliderValue);
            }
        }

        public void SetNormalizedValue(float newNormalizedValue, bool sendCallback = true)
        {
            SetValue(Mathf.Lerp(minValue, maxValue, Mathf.Clamp01(newNormalizedValue)), sendCallback);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsInteractable() && blockPointerWhenDisabled)
            {
                return;
            }

            UpdateDrag(eventData);
            if (onSliderDown != null)
            {
                onSliderDown.Invoke();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (onSliderUp != null)
            {
                onSliderUp.Invoke();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsActive() || (!IsInteractable() && blockPointerWhenDisabled))
            {
                return;
            }

            UpdateDrag(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData != null)
            {
                eventData.useDragThreshold = false;
            }
        }

        public void ApplySliderPresetData(UISliderPreset sourcePreset, UIPresetApplyMask mask)
        {
            if (sourcePreset == null)
            {
                return;
            }

            ApplySelectablePresetData(sourcePreset.stateAnimations, sourcePreset.behaviours, mask);

            if (mask == null || mask.ShouldApplySettings)
            {
                interactable = sourcePreset.interactable;
                minValue = sourcePreset.minValue;
                maxValue = sourcePreset.maxValue;
                wholeNumbers = sourcePreset.wholeNumbers;
                direction = sourcePreset.direction;
                blockPointerWhenDisabled = sourcePreset.blockPointerWhenDisabled;
                invokeOnSubmit = sourcePreset.invokeOnSubmit;
                useInQueue = sourcePreset.useInQueue;
                queueGroup = sourcePreset.queueGroup;
                queueReleaseDelay = sourcePreset.queueReleaseDelay;
                SetValue(sourcePreset.value, false);
            }

            if (mask != null && mask.ShouldApplyTargets)
            {
                fillTarget = sourcePreset.fillTarget;
                handleTarget = sourcePreset.handleTarget;
                UpdateVisuals();
            }

            if (mask != null && mask.ShouldApplyCallbacks)
            {
                onValueChanged = sourcePreset.onValueChanged;
                onSliderDown = sourcePreset.onSliderDown;
                onSliderUp = sourcePreset.onSliderUp;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetValue(sliderValue, false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (maxValue < minValue)
            {
                maxValue = minValue;
            }

            sliderValue = ClampValue(sliderValue);
            UpdateVisuals();
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

        private void UpdateDrag(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            RectTransform clickRect = GetClickRect();
            if (clickRect == null || clickRect.rect.size[(int)GetAxis()] <= 0f)
            {
                return;
            }

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, eventData.pressEventCamera, out localCursor))
            {
                return;
            }

            Rect rect = clickRect.rect;
            int axis = (int)GetAxis();
            float normalized = Mathf.Clamp01((localCursor[axis] - rect.min[axis]) / rect.size[axis]);
            if (IsReversed())
            {
                normalized = 1f - normalized;
            }

            SetNormalizedValue(normalized, true);
        }

        private void UpdateVisuals()
        {
            float visualValue = normalizedValue;
            if (IsReversed())
            {
                visualValue = 1f - visualValue;
            }

            int axis = (int)GetAxis();

            if (fillTarget != null)
            {
                Vector2 anchorMax = fillTarget.anchorMax;
                anchorMax[axis] = visualValue;
                fillTarget.anchorMax = anchorMax;
            }

            if (handleTarget != null)
            {
                Vector2 anchorMin = handleTarget.anchorMin;
                Vector2 anchorMax = handleTarget.anchorMax;
                anchorMin[axis] = visualValue;
                anchorMax[axis] = visualValue;
                handleTarget.anchorMin = anchorMin;
                handleTarget.anchorMax = anchorMax;
            }
        }

        private RectTransform GetClickRect()
        {
            if (fillTarget != null && fillTarget.parent is RectTransform)
            {
                return (RectTransform)fillTarget.parent;
            }

            return transform as RectTransform;
        }

        private RectTransform.Axis GetAxis()
        {
            return direction == Slider.Direction.BottomToTop || direction == Slider.Direction.TopToBottom
                ? RectTransform.Axis.Vertical
                : RectTransform.Axis.Horizontal;
        }

        private bool IsReversed()
        {
            return direction == Slider.Direction.RightToLeft || direction == Slider.Direction.TopToBottom;
        }

        private float ClampValue(float input)
        {
            float clamped = Mathf.Clamp(input, minValue, maxValue);
            return wholeNumbers ? Mathf.Round(clamped) : clamped;
        }
    }
}



