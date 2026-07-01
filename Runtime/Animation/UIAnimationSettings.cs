using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [Serializable]
    [InlineProperty]
    public sealed class UIAnimationSettings
    {
        [HideInInspector]
        public UIAnimationType animationType;

        [HorizontalGroup("Enabled", Width = 90)]
        [LabelText("Enabled")]
        public bool enabled;

        [ShowIf(nameof(enabled))]
        [MinValue(0f)]
        public float delay;

        [ShowIf(nameof(enabled))]
        [MinValue(0f)]
        public float duration = 0.2f;

        [ShowIf(nameof(enabled))]
        public bool useUnscaledTime = true;

        [ShowIf(nameof(enabled))]
        public UIEaseMode easeMode = UIEaseMode.OutQuad;

        [ShowIf("@enabled && easeMode == Project.UI.UIEaseMode.CustomCurve")]
        public AnimationCurve customEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [ShowIf(nameof(enabled))]
        [BoxGroup("Values")]
        public UIValueMode fromMode = UIValueMode.CurrentValue;

        [ShowIf(nameof(ShowVectorFrom))]
        [BoxGroup("Values")]
        public Vector3 customFromVector;

        [ShowIf(nameof(ShowVectorFromOffset))]
        [BoxGroup("Values")]
        public Vector3 fromVectorOffset;

        [ShowIf(nameof(ShowFloatFrom))]
        [BoxGroup("Values")]
        public float customFromFloat;

        [ShowIf(nameof(ShowFloatFromOffset))]
        [BoxGroup("Values")]
        public float fromFloatOffset;

        [ShowIf(nameof(enabled))]
        [BoxGroup("Values")]
        public UIValueMode toMode = UIValueMode.CustomValue;

        [ShowIf(nameof(ShowVectorTo))]
        [BoxGroup("Values")]
        public Vector3 customToVector = Vector3.one;

        [ShowIf(nameof(ShowVectorToOffset))]
        [BoxGroup("Values")]
        public Vector3 toVectorOffset;

        [ShowIf(nameof(ShowFloatTo))]
        [BoxGroup("Values")]
        public float customToFloat = 1f;

        [ShowIf(nameof(ShowFloatToOffset))]
        [BoxGroup("Values")]
        public float toFloatOffset;

        [ShowIf(nameof(enabled))]
        [BoxGroup("Loop")]
        public UIAnimationPlayMode playMode = UIAnimationPlayMode.Once;

        [ShowIf(nameof(ShowLoopSettings))]
        [BoxGroup("Loop")]
        [MinValue(-1)]
        public int loopCount = 1;

        [ShowIf(nameof(ShowLoopSettings))]
        [BoxGroup("Loop")]
        [MinValue(0f)]
        public float loopDelay;

        public UIAnimationSettings()
            : this(UIAnimationType.Move)
        {
        }

        public UIAnimationSettings(UIAnimationType type)
        {
            animationType = type;
            if (type == UIAnimationType.Scale)
            {
                customToVector = Vector3.one;
            }
        }

        public void SetType(UIAnimationType type)
        {
            animationType = type;
        }

        public UIAnimationSettings Clone()
        {
            UIAnimationSettings clone = new UIAnimationSettings(animationType);
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(UIAnimationSettings source)
        {
            if (source == null)
            {
                return;
            }

            animationType = source.animationType;
            enabled = source.enabled;
            delay = source.delay;
            duration = source.duration;
            useUnscaledTime = source.useUnscaledTime;
            easeMode = source.easeMode;
            customEase = source.customEase == null ? null : new AnimationCurve(source.customEase.keys);
            fromMode = source.fromMode;
            customFromVector = source.customFromVector;
            fromVectorOffset = source.fromVectorOffset;
            customFromFloat = source.customFromFloat;
            fromFloatOffset = source.fromFloatOffset;
            toMode = source.toMode;
            customToVector = source.customToVector;
            toVectorOffset = source.toVectorOffset;
            customToFloat = source.customToFloat;
            toFloatOffset = source.toFloatOffset;
            playMode = source.playMode;
            loopCount = source.loopCount;
            loopDelay = source.loopDelay;
        }

        public Vector3 ResolveVectorFrom(Vector3 currentValue, Vector3 startValue)
        {
            return ResolveVector(fromMode, currentValue, startValue, customFromVector, fromVectorOffset);
        }

        public Vector3 ResolveVectorTo(Vector3 currentValue, Vector3 startValue)
        {
            return ResolveVector(toMode, currentValue, startValue, customToVector, toVectorOffset);
        }

        public float ResolveFloatFrom(float currentValue, float startValue)
        {
            return ResolveFloat(fromMode, currentValue, startValue, customFromFloat, fromFloatOffset);
        }

        public float ResolveFloatTo(float currentValue, float startValue)
        {
            return ResolveFloat(toMode, currentValue, startValue, customToFloat, toFloatOffset);
        }

        public float Evaluate(float normalizedTime)
        {
            float t = Mathf.Clamp01(normalizedTime);
            switch (easeMode)
            {
                case UIEaseMode.InQuad:
                    return t * t;
                case UIEaseMode.OutQuad:
                    return t * (2f - t);
                case UIEaseMode.InOutQuad:
                    return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                case UIEaseMode.InCubic:
                    return t * t * t;
                case UIEaseMode.OutCubic:
                    t -= 1f;
                    return t * t * t + 1f;
                case UIEaseMode.InOutCubic:
                    return t < 0.5f ? 4f * t * t * t : 1f + 4f * Mathf.Pow(t - 1f, 3f);
                case UIEaseMode.OutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    t -= 1f;
                    return 1f + c3 * t * t * t + c1 * t * t;
                case UIEaseMode.CustomCurve:
                    return customEase == null ? t : customEase.Evaluate(t);
                default:
                    return t;
            }
        }

        public float GetTotalDuration()
        {
            if (!enabled)
            {
                return 0f;
            }

            int loops = playMode == UIAnimationPlayMode.Once ? 1 : Mathf.Max(1, loopCount);
            if (loopCount < 0 && playMode != UIAnimationPlayMode.Once)
            {
                loops = 1;
            }

            return delay + loops * duration + Mathf.Max(0, loops - 1) * loopDelay;
        }

        private static Vector3 ResolveVector(UIValueMode mode, Vector3 currentValue, Vector3 startValue, Vector3 customValue, Vector3 offset)
        {
            switch (mode)
            {
                case UIValueMode.StartValue:
                    return startValue;
                case UIValueMode.CustomValue:
                    return customValue;
                case UIValueMode.OffsetFromStart:
                    return startValue + offset;
                case UIValueMode.OffsetFromCurrent:
                    return currentValue + offset;
                default:
                    return currentValue;
            }
        }

        private static float ResolveFloat(UIValueMode mode, float currentValue, float startValue, float customValue, float offset)
        {
            switch (mode)
            {
                case UIValueMode.StartValue:
                    return startValue;
                case UIValueMode.CustomValue:
                    return customValue;
                case UIValueMode.OffsetFromStart:
                    return startValue + offset;
                case UIValueMode.OffsetFromCurrent:
                    return currentValue + offset;
                default:
                    return currentValue;
            }
        }

        private bool IsFade()
        {
            return animationType == UIAnimationType.Fade;
        }

        private bool IsVector()
        {
            return animationType != UIAnimationType.Fade;
        }

        private bool ShowVectorFrom()
        {
            return enabled && IsVector() && fromMode == UIValueMode.CustomValue;
        }

        private bool ShowVectorFromOffset()
        {
            return enabled && IsVector() && (fromMode == UIValueMode.OffsetFromStart || fromMode == UIValueMode.OffsetFromCurrent);
        }

        private bool ShowFloatFrom()
        {
            return enabled && IsFade() && fromMode == UIValueMode.CustomValue;
        }

        private bool ShowFloatFromOffset()
        {
            return enabled && IsFade() && (fromMode == UIValueMode.OffsetFromStart || fromMode == UIValueMode.OffsetFromCurrent);
        }

        private bool ShowVectorTo()
        {
            return enabled && IsVector() && toMode == UIValueMode.CustomValue;
        }

        private bool ShowVectorToOffset()
        {
            return enabled && IsVector() && (toMode == UIValueMode.OffsetFromStart || toMode == UIValueMode.OffsetFromCurrent);
        }

        private bool ShowFloatTo()
        {
            return enabled && IsFade() && toMode == UIValueMode.CustomValue;
        }

        private bool ShowFloatToOffset()
        {
            return enabled && IsFade() && (toMode == UIValueMode.OffsetFromStart || toMode == UIValueMode.OffsetFromCurrent);
        }

        private bool ShowLoopSettings()
        {
            return enabled && playMode != UIAnimationPlayMode.Once;
        }
    }
}



