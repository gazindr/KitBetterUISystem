using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    [Serializable]
    public sealed class UIBackgroundSettings
    {
        public bool useBackground;

        [ShowIf(nameof(useBackground))]
        public UIBackground backgroundInstance;

        [ShowIf(nameof(useBackground))]
        public GameObject backgroundPrefab;

        [ShowIf(nameof(useBackground))]
        public bool autoCreate = true;

        [ShowIf(nameof(useBackground))]
        [Tooltip("Behind Container: sibling immediately before this container, so Scale/Move on the container does not affect the dimmer. Inside Container: first child (inherits container transform).")]
        public UIBackgroundAttachMode attachMode = UIBackgroundAttachMode.BehindContainer;

        [ShowIf(nameof(useBackground))]
        public Color backgroundColor = Color.black;

        [ShowIf(nameof(useBackground))]
        [Range(0f, 1f)]
        public float backgroundAlpha = 0.65f;

        [ShowIf(nameof(useBackground))]
        public bool raycastTarget = true;

        [ShowIf(nameof(useBackground))]
        public bool closeContainerOnClick;

        [ShowIf(nameof(useBackground))]
        public bool waitForBackgroundBeforeContainer = true;

        [ShowIf(nameof(useBackground))]
        [MinValue(0f)]
        public float backgroundToContainerDelay;

        [ShowIf(nameof(useBackground))]
        public bool waitForContainerBeforeBackground = true;

        [ShowIf(nameof(useBackground))]
        [MinValue(0f)]
        public float containerToBackgroundDelay;

        [ShowIf(nameof(useBackground))]
        [HideLabel]
        public UIContainerAnimationProfile animations = new UIContainerAnimationProfile();

        public void CopyFrom(UIBackgroundSettings source)
        {
            if (source == null)
            {
                return;
            }

            useBackground = source.useBackground;
            backgroundInstance = source.backgroundInstance;
            backgroundPrefab = source.backgroundPrefab;
            autoCreate = source.autoCreate;
            attachMode = source.attachMode;
            backgroundColor = source.backgroundColor;
            backgroundAlpha = source.backgroundAlpha;
            raycastTarget = source.raycastTarget;
            closeContainerOnClick = source.closeContainerOnClick;
            waitForBackgroundBeforeContainer = source.waitForBackgroundBeforeContainer;
            backgroundToContainerDelay = source.backgroundToContainerDelay;
            waitForContainerBeforeBackground = source.waitForContainerBeforeBackground;
            containerToBackgroundDelay = source.containerToBackgroundDelay;
            animations.CopyFrom(source.animations);
        }
    }

    [AddComponentMenu("UI System/UIBackground")]
    public sealed class UIBackground : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        [HideInInspector]
        private UIContainer owner;

        private UIBackgroundSettings settings;
        private RectTransform cachedRectTransform;
        private Image cachedImage;
        private CanvasGroup cachedCanvasGroup;

        public void Initialize(UIContainer container, UIBackgroundSettings backgroundSettings)
        {
            owner = container;
            settings = backgroundSettings;
            EnsureComponents();
            StretchToScreen();
            ApplyVisuals();
        }

        public void Show(bool instant, Action onComplete)
        {
            EnsureComponents();
            ApplyVisuals();
            gameObject.SetActive(true);
            cachedCanvasGroup.blocksRaycasts = settings == null || settings.raycastTarget;
            cachedCanvasGroup.interactable = cachedCanvasGroup.blocksRaycasts;

            UIAnimationState showAnimation = settings == null || settings.animations == null ? null : settings.animations.show;
            if (showAnimation == null || showAnimation.fade == null || !showAnimation.fade.enabled)
            {
                cachedCanvasGroup.alpha = 1f;
            }

            UIAnimationRunner.PlayState(this, cachedRectTransform, showAnimation, instant, onComplete);
        }

        public void Hide(bool instant, Action onComplete)
        {
            EnsureComponents();
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;

            UIAnimationState hideAnimation = settings == null || settings.animations == null ? null : settings.animations.hide;
            UIAnimationRunner.PlayState(this, cachedRectTransform, hideAnimation, instant, delegate
            {
                if (hideAnimation == null || hideAnimation.fade == null || !hideAnimation.fade.enabled)
                {
                    cachedCanvasGroup.alpha = 0f;
                }

                gameObject.SetActive(false);
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            });
        }

        public void InstantShow()
        {
            Show(true, null);
        }

        public void InstantHide()
        {
            Hide(true, null);
        }

        public void StopAnimations()
        {
            UIAnimationRunner.StopOwner(this);
        }

        public void CompleteAnimations()
        {
            UIAnimationRunner.CompleteOwner(this);
        }

#if UNITY_EDITOR
        public void EditorPreviewShow()
        {
            EnsureComponents();
            ApplyVisuals();
            gameObject.SetActive(true);
            cachedCanvasGroup.blocksRaycasts = settings == null || settings.raycastTarget;
            cachedCanvasGroup.interactable = cachedCanvasGroup.blocksRaycasts;
            UIAnimationState showAnimation = settings == null || settings.animations == null ? null : settings.animations.show;
            UIEditorAnimationPreview.PlayState(this, cachedRectTransform, showAnimation, null);
        }

        public void EditorPreviewHide()
        {
            EnsureComponents();
            cachedCanvasGroup.blocksRaycasts = false;
            cachedCanvasGroup.interactable = false;
            UIAnimationState hideAnimation = settings == null || settings.animations == null ? null : settings.animations.hide;
            UIEditorAnimationPreview.PlayState(this, cachedRectTransform, hideAnimation, null);
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (settings != null && settings.closeContainerOnClick && owner != null)
            {
                owner.Hide();
            }
        }

        private void EnsureComponents()
        {
            if (cachedRectTransform == null)
            {
                cachedRectTransform = transform as RectTransform;
                if (cachedRectTransform == null)
                {
                    cachedRectTransform = gameObject.AddComponent<RectTransform>();
                }
            }

            if (cachedImage == null)
            {
                cachedImage = GetComponent<Image>();
                if (cachedImage == null)
                {
                    cachedImage = gameObject.AddComponent<Image>();
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

        private void ApplyVisuals()
        {
            if (settings == null)
            {
                return;
            }

            Color color = settings.backgroundColor;
            color.a = settings.backgroundAlpha;
            cachedImage.color = color;
            cachedImage.raycastTarget = settings.raycastTarget;
        }

        private void StretchToScreen()
        {
            cachedRectTransform.anchorMin = Vector2.zero;
            cachedRectTransform.anchorMax = Vector2.one;
            cachedRectTransform.offsetMin = Vector2.zero;
            cachedRectTransform.offsetMax = Vector2.zero;
            cachedRectTransform.localScale = Vector3.one;
            cachedRectTransform.localRotation = Quaternion.identity;
        }
    }
}



