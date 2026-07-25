using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    [AddComponentMenu("UI System/UIContainer")]
    public sealed class UIContainer : MonoBehaviour
    {
        [TabGroup("Settings")]
        [ValidateInput(nameof(HasValidId), "Container id should not be empty.")]
        public string id;

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
        [MinValue(0f)]
        public float autoHideDelay = 1f;

        [TabGroup("Settings")]
        [Tooltip("После Hide/InstantHide отключать GameObject (как Doozy InstantHide).")]
        public bool deactivateOnHidden;

        [TabGroup("Settings")]
        [Tooltip("If enabled, this container never plays Show/Hide SFX.")]
        public bool muteUISound;

        [TabGroup("Settings")]
        [ShowIf("@!muteUISound")]
        [Tooltip("Optional clip that overrides the global UI container Show sound from SFXManager.")]
        public AudioClip customShowSound;

        [TabGroup("Settings")]
        [ShowIf("@!muteUISound")]
        [Tooltip("Optional clip that overrides the global UI container Hide sound from SFXManager.")]
        public AudioClip customHideSound;

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

        /// <summary>
        /// Full component preset (settings + animations + background). Source of default values.
        /// </summary>
        public UIContainerPreset preset;

        [SerializeField]
        private List<string> overriddenPaths = new List<string>();

        [SerializeField]
        [HideInInspector]
        private UIContainerState state = UIContainerState.Hidden;

        private Coroutine transitionRoutine;
        private Coroutine autoHideRoutine;
        private RectTransform cachedRectTransform;
        private CanvasGroup cachedCanvasGroup;
        private UIBackground runtimeBackground;

        public event Action<UIContainer> ShowStarted;
        public event Action<UIContainer> Visible;
        public event Action<UIContainer> HideStarted;
        public event Action<UIContainer> Hidden;
        public event Action<UIContainer, bool> VisibilityChanged;

        public UIContainerPreset Preset
        {
            get { return preset; }
            set { preset = value; }
        }

        public List<string> OverriddenPaths
        {
            get { return overriddenPaths; }
        }

        [TabGroup("Debug")]
        [ReadOnly]
        [ShowInInspector]
        public UIContainerState State
        {
            get { return state; }
        }

        /// <summary>Visible или Showing — для совместимости с Doozy UIContainer.isVisible.</summary>
        public bool isVisible
        {
            get { return state == UIContainerState.Visible || state == UIContainerState.Showing; }
        }

        public bool isShowing
        {
            get { return state == UIContainerState.Showing; }
        }

        public bool isHiding
        {
            get { return state == UIContainerState.Hiding; }
        }

        public bool isHidden
        {
            get { return state == UIContainerState.Hidden || state == UIContainerState.Hiding; }
        }

        public bool IsVisible
        {
            get { return isVisible; }
        }

        public string Id
        {
            get { return string.IsNullOrEmpty(id) ? name : id; }
        }

        public bool UseInQueue
        {
            get { return useInQueue; }
        }

        public string QueueGroup
        {
            get { return string.IsNullOrEmpty(queueGroup) ? "Default" : queueGroup; }
        }

        public float QueueShowDelay
        {
            get { return Mathf.Max(0f, queueShowDelay); }
        }

        public void Show()
        {
            StartShow(false, false);
        }

        /// <param name="showCursor">true = UnlockCursor, false = LockCursor. Plain Show() does not touch the cursor.</param>
        public void Show(bool showCursor)
        {
            UICursorBridge.Apply(showCursor);
            Show();
        }

        /// <summary>
        /// Shows this container and hides every other currently open container.
        /// Other Show calls are blocked until this container becomes Hidden,
        /// then previously open containers are restored.
        /// </summary>
        public void ShowIsolated()
        {
            UIContainerIsolationManager.Begin(this);
            if (state == UIContainerState.Visible || state == UIContainerState.Showing)
            {
                return;
            }

            StartShow(false, true);
        }

        /// <param name="showCursor">true = UnlockCursor, false = LockCursor. Plain ShowIsolated() does not touch the cursor.</param>
        public void ShowIsolated(bool showCursor)
        {
            UICursorBridge.Apply(showCursor);
            ShowIsolated();
        }

        public void Hide()
        {
            StartHide(false);
        }

        public void Toggle()
        {
            if (state == UIContainerState.Visible || state == UIContainerState.Showing)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void InstantShow()
        {
            StartShow(true, true);
        }

        public void InstantHide()
        {
            StartHide(true);
        }

        public void StopAnimations()
        {
            UIAnimationRunner.StopOwner(this);
            if (runtimeBackground != null)
            {
                runtimeBackground.StopAnimations();
            }
        }

        public void CompleteAnimations()
        {
            UIAnimationRunner.CompleteOwner(this);
            if (runtimeBackground != null)
            {
                runtimeBackground.CompleteAnimations();
            }
        }

        public static void Show(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.Show();
            }
        }

        public static void Show(string id, bool showCursor)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.Show(showCursor);
            }
        }

        public static void ShowIsolated(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.ShowIsolated();
            }
        }

        public static void ShowIsolated(string id, bool showCursor)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.ShowIsolated(showCursor);
            }
        }

        public static void Hide(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.Hide();
            }
        }

        public static void Toggle(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.Toggle();
            }
        }

        public static void InstantShow(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.InstantShow();
            }
        }

        public static void InstantHide(string id)
        {
            UIContainer container;
            if (TryGetRegistered(id, out container))
            {
                container.InstantHide();
            }
        }

        internal void ShowFromQueue()
        {
            StartShow(false, true);
        }

        public void ApplyContainerPresetData(UIContainerPreset sourcePreset)
        {
            ApplyContainerPresetData(sourcePreset, true);
        }

        public void ApplyContainerPresetData(UIContainerPreset sourcePreset, bool clearOverrides)
        {
            if (sourcePreset == null)
            {
                return;
            }

            category = sourcePreset.category;
            autoRegister = sourcePreset.autoRegister;
            registerOnAwake = sourcePreset.registerOnAwake;
            startupMode = sourcePreset.startupMode;
            useInQueue = sourcePreset.useInQueue;
            queueGroup = sourcePreset.queueGroup;
            queueShowDelay = Mathf.Max(0f, sourcePreset.queueShowDelay);
            useAutoHide = sourcePreset.useAutoHide;
            autoHideDelay = Mathf.Max(0f, sourcePreset.autoHideDelay);
            deactivateOnHidden = sourcePreset.deactivateOnHidden;
            muteUISound = sourcePreset.muteUISound;
            customShowSound = sourcePreset.customShowSound;
            customHideSound = sourcePreset.customHideSound;

            if (animations == null)
            {
                animations = new UIContainerAnimationProfile();
            }

            animations.CopyFrom(sourcePreset.animations);

            if (backgroundSettings == null)
            {
                backgroundSettings = new UIBackgroundSettings();
            }

            backgroundSettings.CopyFrom(sourcePreset.backgroundSettings);

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
                ApplyContainerPresetData(preset, false);
                return;
            }

#if UNITY_EDITOR
            UIPresetOverrideSync.ApplyNonOverridden(this, preset, overriddenPaths);
#else
            ApplyContainerPresetData(preset, false);
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

        private void Reset()
        {
            id = name;
            animations = UIAnimationDefaults.CreateContainerProfile();
            backgroundSettings = new UIBackgroundSettings();
            overriddenPaths = new List<string>();

            UIContainerPreset defaultPreset = null;
            UISystemDefaults defaults = UISystemDefaults.Instance;
            if (defaults != null)
            {
                defaultPreset = defaults.defaultContainerPreset;
            }

            if (defaultPreset == null)
            {
                defaultPreset = Resources.Load<UIContainerPreset>("Default-UIContainerPreset");
            }

            if (defaultPreset != null)
            {
                preset = defaultPreset;
                ApplyContainerPresetData(defaultPreset, true);
            }
        }

        private void Awake()
        {
            EnsureCanvasGroup();
            if (autoRegister && registerOnAwake)
            {
                UIRegistry.Register(this);
            }
        }

        private void OnEnable()
        {
            if (autoRegister && !registerOnAwake)
            {
                UIRegistry.Register(this);
            }
        }

        private void Start()
        {
            ApplyStartupMode();
        }

        private void OnDisable()
        {
            StopTransitionRoutine();
            StopAutoHideRoutine();
            StopAnimations();
            if (state != UIContainerState.Hidden)
            {
                state = UIContainerState.Hidden;
                UIContainerQueueManager.NotifyHidden(this);
                UIContainerIsolationManager.NotifyHidden(this);
            }
        }

        private void OnDestroy()
        {
            UIContainerIsolationManager.Remove(this);
            UIContainerQueueManager.Remove(this);
            UIRegistry.Unregister(this);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }

            queueShowDelay = Mathf.Max(0f, queueShowDelay);
            autoHideDelay = Mathf.Max(0f, autoHideDelay);

            if (animations != null)
            {
                animations.show.EnsureTypes();
                animations.hide.EnsureTypes();
            }

            if (backgroundSettings != null && backgroundSettings.animations != null)
            {
                backgroundSettings.animations.show.EnsureTypes();
                backgroundSettings.animations.hide.EnsureTypes();
            }
        }

        private void StartShow(bool instant, bool fromQueue)
        {
            if (UIContainerIsolationManager.IsBlocked(this))
            {
                return;
            }

            if (!fromQueue && !instant)
            {
                if (useInQueue)
                {
                    if (UIContainerQueueManager.RequestShow(this))
                    {
                        return;
                    }
                }
                else if (state == UIContainerState.Visible || state == UIContainerState.Showing)
                {
                    // Already visible / showing — ignore repeat Show until Hidden.
                    return;
                }
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            StopTransitionRoutine();
            StopAutoHideRoutine();
            UIAnimationRunner.StopOwner(this);
            PlayShowSound();
            transitionRoutine = StartCoroutine(ShowRoutine(instant));
        }

        private void StartHide(bool instant)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            bool shouldPlayHideSound = state == UIContainerState.Visible || state == UIContainerState.Showing;
            StopTransitionRoutine();
            StopAutoHideRoutine();
            UIAnimationRunner.StopOwner(this);
            if (shouldPlayHideSound)
            {
                PlayHideSound();
            }

            transitionRoutine = StartCoroutine(HideRoutine(instant));
        }

        private void PlayShowSound()
        {
            UISFX.Play(UISFXKind.ContainerShow, customShowSound, muteUISound);
        }

        private void PlayHideSound()
        {
            UISFX.Play(UISFXKind.ContainerHide, customHideSound, muteUISound);
        }

        private IEnumerator ShowRoutine(bool instant)
        {
            state = UIContainerState.Showing;
            EnsureCanvasGroup();
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;

            if (animations == null || animations.show == null || animations.show.fade == null || !animations.show.fade.enabled)
            {
                cachedCanvasGroup.alpha = 1f;
            }

            if (onShow != null)
            {
                onShow.Invoke();
            }

            if (ShowStarted != null)
            {
                ShowStarted.Invoke(this);
            }

            bool backgroundDone = true;
            UIBackground background = PrepareBackground();
            if (backgroundSettings != null && backgroundSettings.useBackground && background != null)
            {
                backgroundDone = false;
                background.Show(instant, delegate { backgroundDone = true; });
            }

            if (backgroundSettings != null && backgroundSettings.useBackground && backgroundSettings.waitForBackgroundBeforeContainer)
            {
                while (!backgroundDone)
                {
                    yield return null;
                }
            }

            if (backgroundSettings != null && backgroundSettings.useBackground && backgroundSettings.backgroundToContainerDelay > 0f)
            {
                yield return WaitUnscaled(backgroundSettings.backgroundToContainerDelay);
            }

            bool containerDone = false;
            UIAnimationRunner.PlayState(this, cachedRectTransform, animations == null ? null : animations.show, instant, delegate { containerDone = true; });
            while (!containerDone)
            {
                yield return null;
            }

            while (!backgroundDone)
            {
                yield return null;
            }

            state = UIContainerState.Visible;
            cachedCanvasGroup.alpha = Mathf.Max(cachedCanvasGroup.alpha, 0.0001f);
            cachedCanvasGroup.blocksRaycasts = true;
            cachedCanvasGroup.interactable = true;

            if (onVisible != null)
            {
                onVisible.Invoke();
            }

            if (Visible != null)
            {
                Visible.Invoke(this);
            }

            if (visibilityChanged != null)
            {
                visibilityChanged.Invoke(true);
            }

            if (VisibilityChanged != null)
            {
                VisibilityChanged.Invoke(this, true);
            }

            StartAutoHideRoutine();
            transitionRoutine = null;
        }

        private IEnumerator HideRoutine(bool instant)
        {
            state = UIContainerState.Hiding;
            EnsureCanvasGroup();
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;

            if (onHide != null)
            {
                onHide.Invoke();
            }

            if (HideStarted != null)
            {
                HideStarted.Invoke(this);
            }

            bool containerDone = false;
            UIAnimationRunner.PlayState(this, cachedRectTransform, animations == null ? null : animations.hide, instant, delegate { containerDone = true; });

            bool backgroundDone = true;
            UIBackground background = runtimeBackground;

            if (backgroundSettings != null && backgroundSettings.useBackground && background != null && !backgroundSettings.waitForContainerBeforeBackground)
            {
                backgroundDone = false;
                background.Hide(instant, delegate { backgroundDone = true; });
            }

            if (backgroundSettings == null || !backgroundSettings.useBackground || backgroundSettings.waitForContainerBeforeBackground)
            {
                while (!containerDone)
                {
                    yield return null;
                }
            }

            if (backgroundSettings != null && backgroundSettings.useBackground && backgroundSettings.containerToBackgroundDelay > 0f)
            {
                yield return WaitUnscaled(backgroundSettings.containerToBackgroundDelay);
            }

            if (backgroundSettings != null && backgroundSettings.useBackground && background != null && backgroundSettings.waitForContainerBeforeBackground)
            {
                backgroundDone = false;
                background.Hide(instant, delegate { backgroundDone = true; });
            }

            while (!containerDone || !backgroundDone)
            {
                yield return null;
            }

            if (animations == null || animations.hide == null || animations.hide.fade == null || !animations.hide.fade.enabled)
            {
                cachedCanvasGroup.alpha = 0f;
            }

            state = UIContainerState.Hidden;

            if (onHidden != null)
            {
                onHidden.Invoke();
            }

            if (Hidden != null)
            {
                Hidden.Invoke(this);
            }

            if (visibilityChanged != null)
            {
                visibilityChanged.Invoke(false);
            }

            if (VisibilityChanged != null)
            {
                VisibilityChanged.Invoke(this, false);
            }

            UIContainerQueueManager.NotifyHidden(this);
            UIContainerIsolationManager.NotifyHidden(this);
            transitionRoutine = null;

            if (deactivateOnHidden)
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyStartupMode()
        {
            switch (startupMode)
            {
                case UIContainerStartupMode.InstantShow:
                    InstantShow();
                    break;
                case UIContainerStartupMode.Hide:
                    Hide();
                    break;
                case UIContainerStartupMode.Show:
                    Show();
                    break;
                default:
                    InstantHide();
                    break;
            }
        }

        private UIBackground PrepareBackground()
        {
            if (backgroundSettings == null || !backgroundSettings.useBackground)
            {
                return null;
            }

            if (backgroundSettings.backgroundInstance != null)
            {
                runtimeBackground = backgroundSettings.backgroundInstance;
            }

            if (runtimeBackground == null && backgroundSettings.backgroundPrefab != null)
            {
                GameObject backgroundObject = Instantiate(backgroundSettings.backgroundPrefab, transform);
                backgroundObject.name = backgroundSettings.backgroundPrefab.name + " (UI System)";
                runtimeBackground = backgroundObject.GetComponent<UIBackground>();
                if (runtimeBackground == null)
                {
                    runtimeBackground = backgroundObject.AddComponent<UIBackground>();
                }
            }

            if (runtimeBackground == null && backgroundSettings.autoCreate)
            {
                GameObject backgroundObject = new GameObject(name + " Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(UIBackground));
                backgroundObject.transform.SetParent(transform, false);
                runtimeBackground = backgroundObject.GetComponent<UIBackground>();
            }

            if (runtimeBackground == null)
            {
                return null;
            }

            // Always keep background as the first child so it draws behind container content.
            if (runtimeBackground.transform.parent != transform)
            {
                runtimeBackground.transform.SetParent(transform, false);
            }

            runtimeBackground.transform.SetAsFirstSibling();
            runtimeBackground.Initialize(this, backgroundSettings);
            return runtimeBackground;
        }

        private void EnsureCanvasGroup()
        {
            if (cachedRectTransform == null)
            {
                cachedRectTransform = transform as RectTransform;
                if (cachedRectTransform == null)
                {
                    cachedRectTransform = gameObject.AddComponent<RectTransform>();
                }
            }

            if (cachedCanvasGroup == null)
            {
                cachedCanvasGroup = GetComponent<CanvasGroup>();
                if (cachedCanvasGroup == null)
                {
                    cachedCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void StartAutoHideRoutine()
        {
            StopAutoHideRoutine();
            if (useAutoHide && autoHideDelay > 0f)
            {
                autoHideRoutine = StartCoroutine(AutoHideRoutine());
            }
        }

        private IEnumerator AutoHideRoutine()
        {
            yield return WaitUnscaled(autoHideDelay);
            autoHideRoutine = null;
            Hide();
        }

        private static IEnumerator WaitUnscaled(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void StopTransitionRoutine()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }
        }

        private void StopAutoHideRoutine()
        {
            if (autoHideRoutine != null)
            {
                StopCoroutine(autoHideRoutine);
                autoHideRoutine = null;
            }
        }

        private bool HasValidId(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        private static bool TryGetRegistered(string id, out UIContainer container)
        {
            if (UIRegistry.TryGet(id, out container))
            {
                return true;
            }

            Debug.LogWarning("[UISystem] UIContainer with id '" + id + "' is not registered.");
            return false;
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.85f, 0.5f)]
        private void RegisterNow()
        {
            UIRegistry.Register(this);
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Medium)]
        private void PreviewShow()
        {
#if UNITY_EDITOR
            EditorPreviewShowAnimation();
#else
            Show();
#endif
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Medium)]
        private void PreviewHide()
        {
#if UNITY_EDITOR
            EditorPreviewHideAnimation();
#else
            Hide();
#endif
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        private void CaptureCurrentAsStart()
        {
            EnsureCanvasGroup();
            animations.show.CaptureCurrentAsStart(cachedRectTransform, cachedCanvasGroup);
            animations.hide.CaptureCurrentAsStart(cachedRectTransform, cachedCanvasGroup);
            UIAnimationRunner.CaptureStart(cachedRectTransform);
        }

        [TabGroup("Debug")]
        [Button(ButtonSizes.Small)]
        private void ResetAnimations()
        {
            animations = UIAnimationDefaults.CreateContainerProfile();
            if (preset != null && preset.animations != null)
            {
                animations.CopyFrom(preset.animations);
            }

            if (overriddenPaths != null && overriddenPaths.Count > 0)
            {
                List<string> keep = new List<string>();
                for (int i = 0; i < overriddenPaths.Count; i++)
                {
                    string path = overriddenPaths[i];
                    if (path != null && !path.StartsWith("animations.", StringComparison.Ordinal))
                    {
                        keep.Add(path);
                    }
                }

                overriddenPaths.Clear();
                overriddenPaths.AddRange(keep);
            }
        }

        public void ApplyPresetFromInspector()
        {
            if (preset != null)
            {
                ApplyContainerPresetData(preset, true);
            }
        }

        public void SaveAllToPreset()
        {
            if (preset == null)
            {
                return;
            }

            preset.category = category;
            preset.autoRegister = autoRegister;
            preset.registerOnAwake = registerOnAwake;
            preset.startupMode = startupMode;
            preset.useInQueue = useInQueue;
            preset.queueGroup = queueGroup;
            preset.queueShowDelay = queueShowDelay;
            preset.useAutoHide = useAutoHide;
            preset.autoHideDelay = autoHideDelay;
            preset.deactivateOnHidden = deactivateOnHidden;
            preset.muteUISound = muteUISound;
            preset.customShowSound = customShowSound;
            preset.customHideSound = customHideSound;

            if (preset.animations == null)
            {
                preset.animations = new UIContainerAnimationProfile();
            }

            preset.animations.CopyFrom(animations);

            if (preset.backgroundSettings == null)
            {
                preset.backgroundSettings = new UIBackgroundSettings();
            }

            preset.backgroundSettings.CopyFrom(backgroundSettings);
            UIPresetOverrideUtility.ClearOverrides(overriddenPaths);
        }

#if UNITY_EDITOR
        public void EditorPreviewShowAnimation()
        {
            EnsureCanvasGroup();
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            state = UIContainerState.Showing;
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;

            UIBackground background = PrepareBackground();
            if (backgroundSettings != null && backgroundSettings.useBackground && background != null)
            {
                background.EditorPreviewShow();
            }

            UIEditorAnimationPreview.PlayState(this, cachedRectTransform, animations == null ? null : animations.show, delegate
            {
                state = UIContainerState.Visible;
                cachedCanvasGroup.blocksRaycasts = true;
                cachedCanvasGroup.interactable = true;
            });
        }

        public void EditorPreviewHideAnimation()
        {
            EnsureCanvasGroup();
            state = UIContainerState.Hiding;
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;

            UIEditorAnimationPreview.PlayState(this, cachedRectTransform, animations == null ? null : animations.hide, delegate
            {
                state = UIContainerState.Hidden;
            });

            if (backgroundSettings != null && backgroundSettings.useBackground && runtimeBackground != null)
            {
                runtimeBackground.EditorPreviewHide();
            }
        }

        public void EditorStopPreview()
        {
            UIEditorAnimationPreview.StopOwner(this);
            if (runtimeBackground != null)
            {
                runtimeBackground.EditorStopPreview();
            }
        }

        public void EditorCompletePreview()
        {
            UIEditorAnimationPreview.CompleteOwner(this);
            if (runtimeBackground != null)
            {
                runtimeBackground.EditorCompletePreview();
            }
        }
#endif
    }
}



