using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    public abstract class UISelectable : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [TabGroup("Settings")]
        public bool animateStateAutomatically = true;

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
        [MinValue(0f)]
        public float queueReleaseDelay = 0.05f;

        [TabGroup("Settings")]
        [MinValue(0.01f)]
        public float doubleClickInterval = 0.3f;

        [TabGroup("Settings")]
        [MinValue(0.01f)]
        public float longClickDuration = 0.6f;

        [TabGroup("States")]
        [HideLabel]
        public UISelectableAnimationProfile stateAnimations = new UISelectableAnimationProfile();

        [TabGroup("Behaviours")]
        [HideInInspector]
        public bool allowDuplicateBehaviourTriggers;

        [TabGroup("Behaviours")]
        [ValueDropdown(nameof(GetAvailableBehaviourTriggers))]
        public UIBehaviourTrigger behaviourToAdd = UIBehaviourTrigger.PointerLeftClick;

        [TabGroup("Behaviours")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        [TabGroup("Debug")]
        [ReadOnly]
        [ShowInInspector]
        public UISelectableState CurrentState
        {
            get { return currentState; }
        }

        [TabGroup("Debug")]
        public UISelectableState previewState = UISelectableState.Normal;

        [SerializeField]
        [HideInInspector]
        private UISelectableState currentState = UISelectableState.Normal;

        private Coroutine longClickRoutine;
        private bool pointerIsDown;
        private bool isExecutingQueuedTrigger;
        private float lastLeftClickTime = -999999f;

        public virtual void SetState(UISelectableState state, bool instant = false)
        {
            UISelectableState previousState = currentState;
            currentState = state;
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null || stateAnimations == null)
            {
                return;
            }

            UIAnimationState animationState = stateAnimations.GetState(state);
            UIAnimationState previousAnimationState = stateAnimations.GetState(previousState);
            UIAnimationRunner.PlayStateTransition(this, rectTransform, animationState, previousAnimationState, instant, null);
        }

        public virtual void SetInteractable(bool value)
        {
            interactable = value;
            SetState(value ? UISelectableState.Normal : UISelectableState.Disabled, true);
        }

        public virtual void ExecuteTrigger(UIBehaviourTrigger trigger)
        {
            ExecuteTrigger(trigger, null);
        }

        public virtual void StopAnimations()
        {
            UIAnimationRunner.StopOwner(this);
        }

        public virtual void CompleteAnimations()
        {
            UIAnimationRunner.CompleteOwner(this);
        }

        public virtual void ApplySelectablePresetData(UISelectableAnimationProfile animations, List<UIBehaviourBlock> presetBehaviours, UIPresetApplyMask mask)
        {
            if (mask == null)
            {
                mask = new UIPresetApplyMask();
            }

            if (mask.ShouldApplyAnimations && animations != null)
            {
                stateAnimations.CopyFrom(animations);
            }

            if (mask.ShouldApplyBehaviours && presetBehaviours != null)
            {
                behaviours = CloneBehaviourBlocks(presetBehaviours, mask.ShouldApplyCallbacks);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (CanExecutePointerTrigger())
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerEnter, eventData);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (CanExecutePointerTrigger())
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerExit, eventData);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!CanExecutePointerTrigger())
            {
                return;
            }

            pointerIsDown = true;
            StartLongClickWatch(eventData);
            ExecuteTrigger(UIBehaviourTrigger.PointerDown, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            pointerIsDown = false;
            StopLongClickWatch();
            if (CanExecutePointerTrigger())
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerUp, eventData);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!CanExecutePointerTrigger())
            {
                return;
            }

            if (eventData == null)
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerLeftClick, null);
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerLeftClick, eventData);
                float now = Time.unscaledTime;
                if (now - lastLeftClickTime <= doubleClickInterval)
                {
                    ExecuteTrigger(UIBehaviourTrigger.PointerDoubleClick, eventData);
                    lastLeftClickTime = -999999f;
                }
                else
                {
                    lastLeftClickTime = now;
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerMiddleClick, eventData);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerRightClick, eventData);
            }
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (invokeOnSubmit && CanExecutePointerTrigger())
            {
                ExecuteTrigger(UIBehaviourTrigger.Submit, null);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            ExecuteTrigger(UIBehaviourTrigger.Selected, null);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            ExecuteTrigger(UIBehaviourTrigger.Deselected, null);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            if (animateStateAutomatically)
            {
                SetState(MapSelectionState(state), instant);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetBehaviourRuntimeState();
            SetState(IsInteractable() ? UISelectableState.Normal : UISelectableState.Disabled, true);
        }

        protected override void OnDisable()
        {
            pointerIsDown = false;
            StopLongClickWatch();
            StopAnimations();
            base.OnDisable();
        }

        protected virtual void Update()
        {
            PollKeyboardBehaviours();
        }

        /// <summary>
        /// Выполнить один behaviour block (используется для keyboard hotkey и ручного вызова).
        /// </summary>
        public void ExecuteBehaviourBlock(UIBehaviourBlock block)
        {
            if (block == null)
            {
                return;
            }

            UIBehaviourContext context = UIBehaviourContext.Create(this, block.trigger, null);
            block.Execute(context, this, IsInteractable());
        }

        private void PollKeyboardBehaviours()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                return;
            }

            if (behaviours == null || behaviours.Count == 0)
            {
                return;
            }

            for (int i = 0; i < behaviours.Count; i++)
            {
                UIBehaviourBlock block = behaviours[i];
                if (block == null || block.keyboardKey == KeyCode.None)
                {
                    continue;
                }

                if (!Input.GetKeyDown(block.keyboardKey))
                {
                    continue;
                }

                ExecuteBehaviourBlock(block);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (stateAnimations != null)
            {
                stateAnimations.normal.EnsureTypes();
                stateAnimations.highlighted.EnsureTypes();
                stateAnimations.pressed.EnsureTypes();
                stateAnimations.selected.EnsureTypes();
                stateAnimations.disabled.EnsureTypes();
            }
        }

        protected void ExecuteTrigger(UIBehaviourTrigger trigger, PointerEventData pointerEventData, bool hasSelectedState = false, UISelectableState selectedState = UISelectableState.Normal, bool hasSliderValue = false, float sliderValue = 0f)
        {
            if (useInQueue && !isExecutingQueuedTrigger)
            {
                float releaseDelay = GetEstimatedTriggerDuration(trigger) + queueReleaseDelay;
                UIInteractionQueueManager.Enqueue(this, queueGroup, delegate
                {
                    isExecutingQueuedTrigger = true;
                    ExecuteTriggerInternal(trigger, pointerEventData, hasSelectedState, selectedState, hasSliderValue, sliderValue);
                    isExecutingQueuedTrigger = false;
                }, releaseDelay);
                return;
            }

            ExecuteTriggerInternal(trigger, pointerEventData, hasSelectedState, selectedState, hasSliderValue, sliderValue);
        }

        private void ExecuteTriggerInternal(UIBehaviourTrigger trigger, PointerEventData pointerEventData, bool hasSelectedState, UISelectableState selectedState, bool hasSliderValue, float sliderValue)
        {
            if (behaviours == null || behaviours.Count == 0)
            {
                return;
            }

            UIBehaviourContext context = UIBehaviourContext.Create(this, trigger, pointerEventData);
            context.hasSelectedState = hasSelectedState;
            context.selectedState = selectedState;
            context.hasSliderValue = hasSliderValue;
            context.sliderValue = sliderValue;

            bool sourceInteractable = IsInteractable();
            for (int i = 0; i < behaviours.Count; i++)
            {
                UIBehaviourBlock block = behaviours[i];
                if (block != null && block.trigger == trigger)
                {
                    block.Execute(context, this, sourceInteractable);
                }
            }
        }

        private float GetEstimatedTriggerDuration(UIBehaviourTrigger trigger)
        {
            if (behaviours == null)
            {
                return 0f;
            }

            float duration = 0f;
            for (int i = 0; i < behaviours.Count; i++)
            {
                UIBehaviourBlock block = behaviours[i];
                if (block != null && block.trigger == trigger)
                {
                    duration = Mathf.Max(duration, block.GetEstimatedDuration());
                }
            }

            return duration;
        }

        [TabGroup("Behaviours")]
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.85f, 0.5f)]
        protected void AddBehaviour()
        {
            AddBehaviourBlock(behaviourToAdd);
        }

        public void AddBehaviourBlock(UIBehaviourTrigger trigger)
        {
            if (behaviours == null)
            {
                behaviours = new List<UIBehaviourBlock>();
            }

            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i] != null && behaviours[i].trigger == trigger)
                {
                    Debug.LogWarning("[UISystem] Behaviour trigger '" + trigger + "' already exists on " + name + ".");
                    return;
                }
            }

            UIBehaviourBlock block = new UIBehaviourBlock();
            block.trigger = trigger;
            block.allowDuplicates = false;
            block.entries.Add(new UIBehaviourEntry { name = block.GetDisplayName() });
            behaviours.Add(block);
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Medium)]
        protected void PreviewState()
        {
            SetState(previewState, true);
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        protected void CaptureCurrentAsStart()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null || stateAnimations == null)
            {
                return;
            }

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            stateAnimations.GetState(previewState).CaptureCurrentAsStart(rectTransform, canvasGroup);
            UIAnimationRunner.CaptureStart(rectTransform);
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        protected void CaptureCurrentAsCustomFrom()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null || stateAnimations == null)
            {
                return;
            }

            stateAnimations.GetState(previewState).CaptureCurrentAsCustomFrom(rectTransform, GetComponent<CanvasGroup>());
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        protected void CaptureCurrentAsCustomTo()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null || stateAnimations == null)
            {
                return;
            }

            stateAnimations.GetState(previewState).CaptureCurrentAsCustomTo(rectTransform, GetComponent<CanvasGroup>());
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        protected void ResetAnimations()
        {
            stateAnimations = UIAnimationDefaults.CreateButtonProfile();
        }

        protected static List<UIBehaviourBlock> CloneBehaviourBlocks(List<UIBehaviourBlock> source, bool includeCallbacks)
        {
            List<UIBehaviourBlock> result = new List<UIBehaviourBlock>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    result.Add(source[i].Clone(includeCallbacks));
                }
            }

            return result;
        }

        private static UISelectableState MapSelectionState(SelectionState state)
        {
            switch (state)
            {
                case SelectionState.Highlighted:
                    return UISelectableState.Highlighted;
                case SelectionState.Pressed:
                    return UISelectableState.Pressed;
                case SelectionState.Selected:
                    return UISelectableState.Selected;
                case SelectionState.Disabled:
                    return UISelectableState.Disabled;
                default:
                    return UISelectableState.Normal;
            }
        }

        private bool CanExecutePointerTrigger()
        {
            return IsInteractable() || !blockPointerWhenDisabled;
        }

        private void StartLongClickWatch(PointerEventData eventData)
        {
            StopLongClickWatch();
            if (longClickDuration <= 0f)
            {
                return;
            }

            longClickRoutine = StartCoroutine(LongClickWatch(eventData));
        }

        private void StopLongClickWatch()
        {
            if (longClickRoutine != null)
            {
                StopCoroutine(longClickRoutine);
                longClickRoutine = null;
            }
        }

        private IEnumerator LongClickWatch(PointerEventData eventData)
        {
            float elapsed = 0f;
            while (elapsed < longClickDuration)
            {
                if (!pointerIsDown)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (pointerIsDown)
            {
                ExecuteTrigger(UIBehaviourTrigger.PointerLongClick, eventData);
            }
        }

        private void ResetBehaviourRuntimeState()
        {
            if (behaviours == null)
            {
                return;
            }

            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviours[i].ResetRuntimeState();
                }
            }
        }

        private static IEnumerable<UIBehaviourTrigger> GetAvailableBehaviourTriggers()
        {
            return (UIBehaviourTrigger[])Enum.GetValues(typeof(UIBehaviourTrigger));
        }

#if UNITY_EDITOR
        public void EditorPreviewState(UISelectableState state)
        {
            currentState = state;
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null || stateAnimations == null)
            {
                return;
            }

            UIEditorAnimationPreview.PlayState(this, rectTransform, stateAnimations.GetState(state), null);
        }

        public void EditorStopPreview()
        {
            UIEditorAnimationPreview.StopOwner(this);
        }

        public void EditorCompletePreview()
        {
            UIEditorAnimationPreview.CompleteOwner(this);
        }
#endif
    }
}



