using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [Serializable]
    [InlineProperty]
    public sealed class UIAnimationState
    {
        [TabGroup("Move")]
        [HideLabel]
        public UIAnimationSettings move = new UIAnimationSettings(UIAnimationType.Move);

        [TabGroup("Rotate")]
        [HideLabel]
        public UIAnimationSettings rotate = new UIAnimationSettings(UIAnimationType.Rotate);

        [TabGroup("Scale")]
        [HideLabel]
        public UIAnimationSettings scale = new UIAnimationSettings(UIAnimationType.Scale);

        [TabGroup("Fade")]
        [HideLabel]
        [InfoBox("Fade uses CanvasGroup.alpha. CanvasGroup is added automatically at runtime when needed.", InfoMessageType.Info)]
        public UIAnimationSettings fade = new UIAnimationSettings(UIAnimationType.Fade);

        public bool HasEnabledAnimations
        {
            get
            {
                EnsureTypes();
                return move.enabled || rotate.enabled || scale.enabled || fade.enabled;
            }
        }

        public void EnsureTypes()
        {
            if (move == null)
            {
                move = new UIAnimationSettings(UIAnimationType.Move);
            }

            if (rotate == null)
            {
                rotate = new UIAnimationSettings(UIAnimationType.Rotate);
            }

            if (scale == null)
            {
                scale = new UIAnimationSettings(UIAnimationType.Scale);
            }

            if (fade == null)
            {
                fade = new UIAnimationSettings(UIAnimationType.Fade);
            }

            move.SetType(UIAnimationType.Move);
            rotate.SetType(UIAnimationType.Rotate);
            scale.SetType(UIAnimationType.Scale);
            fade.SetType(UIAnimationType.Fade);
        }

        public UIAnimationState Clone()
        {
            UIAnimationState clone = new UIAnimationState();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(UIAnimationState source)
        {
            EnsureTypes();
            if (source == null)
            {
                return;
            }

            source.EnsureTypes();
            move.CopyFrom(source.move);
            rotate.CopyFrom(source.rotate);
            scale.CopyFrom(source.scale);
            fade.CopyFrom(source.fade);
        }

        [Button(ButtonSizes.Small)]
        public void ResetAnimations()
        {
            move = new UIAnimationSettings(UIAnimationType.Move);
            rotate = new UIAnimationSettings(UIAnimationType.Rotate);
            scale = new UIAnimationSettings(UIAnimationType.Scale);
            fade = new UIAnimationSettings(UIAnimationType.Fade);
        }

        public void CaptureCurrentAsStart(RectTransform target, CanvasGroup canvasGroup)
        {
            CaptureCurrent(target, canvasGroup, true);
        }

        public void CaptureCurrentAsCustomFrom(RectTransform target, CanvasGroup canvasGroup)
        {
            CaptureCurrent(target, canvasGroup, true);
        }

        public void CaptureCurrentAsCustomTo(RectTransform target, CanvasGroup canvasGroup)
        {
            CaptureCurrent(target, canvasGroup, false);
        }

        private void CaptureCurrent(RectTransform target, CanvasGroup canvasGroup, bool captureFrom)
        {
            EnsureTypes();
            if (target == null)
            {
                return;
            }

            if (captureFrom)
            {
                move.fromMode = UIValueMode.CustomValue;
                move.customFromVector = target.anchoredPosition3D;
                rotate.fromMode = UIValueMode.CustomValue;
                rotate.customFromVector = target.localEulerAngles;
                scale.fromMode = UIValueMode.CustomValue;
                scale.customFromVector = target.localScale;
                fade.fromMode = UIValueMode.CustomValue;
                fade.customFromFloat = canvasGroup == null ? 1f : canvasGroup.alpha;
            }
            else
            {
                move.toMode = UIValueMode.CustomValue;
                move.customToVector = target.anchoredPosition3D;
                rotate.toMode = UIValueMode.CustomValue;
                rotate.customToVector = target.localEulerAngles;
                scale.toMode = UIValueMode.CustomValue;
                scale.customToVector = target.localScale;
                fade.toMode = UIValueMode.CustomValue;
                fade.customToFloat = canvasGroup == null ? 1f : canvasGroup.alpha;
            }
        }
    }
}



