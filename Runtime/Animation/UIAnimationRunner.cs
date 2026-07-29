using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    public static class UIAnimationRunner
    {
        private const string RunnerName = "[UI System Animation Runner]";
        private static UIAnimationTweenRunner runner;
        private static readonly Dictionary<string, ActiveTween> ActiveByKey = new Dictionary<string, ActiveTween>();
        private static readonly List<ActiveTween> ActiveTweens = new List<ActiveTween>(32);
        private static readonly List<ActiveTween> ScratchTweens = new List<ActiveTween>(32);
        private static readonly Dictionary<int, StartValues> StartValuesByTarget = new Dictionary<int, StartValues>();

        public static void PlayState(UnityEngine.Object owner, RectTransform target, UIAnimationState state, bool instant, Action onComplete)
        {
            if (target == null || state == null)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            state.EnsureTypes();
            CanvasGroup canvasGroup = NeedsCanvasGroup(state) ? EnsureCanvasGroup(target) : target.GetComponent<CanvasGroup>();
            StartValues startValues = GetStartValues(target, canvasGroup);

            int pending = 0;
            bool hasAnimation = false;

            Action propertyComplete = delegate
            {
                pending--;
                if (pending <= 0 && onComplete != null)
                {
                    onComplete.Invoke();
                }
            };

            PlayProperty(owner, target, canvasGroup, startValues, state.move, UIAnimationType.Move, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayProperty(owner, target, canvasGroup, startValues, state.rotate, UIAnimationType.Rotate, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayProperty(owner, target, canvasGroup, startValues, state.scale, UIAnimationType.Scale, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayProperty(owner, target, canvasGroup, startValues, state.fade, UIAnimationType.Fade, instant, ref pending, ref hasAnimation, propertyComplete);

            if (pending <= 0 && onComplete != null)
            {
                onComplete.Invoke();
            }
        }

        public static void PlayStateTransition(UnityEngine.Object owner, RectTransform target, UIAnimationState state, UIAnimationState previousState, bool instant, Action onComplete)
        {
            if (target == null || state == null)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            state.EnsureTypes();
            if (previousState != null)
            {
                previousState.EnsureTypes();
            }

            bool needsCanvasGroup = NeedsCanvasGroup(state) || NeedsCanvasGroup(previousState);
            CanvasGroup canvasGroup = needsCanvasGroup ? EnsureCanvasGroup(target) : target.GetComponent<CanvasGroup>();
            StartValues startValues = GetStartValues(target, canvasGroup);

            int pending = 0;
            bool hasAnimation = false;

            Action propertyComplete = delegate
            {
                pending--;
                if (pending <= 0 && onComplete != null)
                {
                    onComplete.Invoke();
                }
            };

            PlayTransitionProperty(owner, target, canvasGroup, startValues, state.move, previousState == null ? null : previousState.move, UIAnimationType.Move, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayTransitionProperty(owner, target, canvasGroup, startValues, state.rotate, previousState == null ? null : previousState.rotate, UIAnimationType.Rotate, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayTransitionProperty(owner, target, canvasGroup, startValues, state.scale, previousState == null ? null : previousState.scale, UIAnimationType.Scale, instant, ref pending, ref hasAnimation, propertyComplete);
            PlayTransitionProperty(owner, target, canvasGroup, startValues, state.fade, previousState == null ? null : previousState.fade, UIAnimationType.Fade, instant, ref pending, ref hasAnimation, propertyComplete);

            if (pending <= 0 && onComplete != null)
            {
                onComplete.Invoke();
            }
        }

        public static void StopOwner(UnityEngine.Object owner)
        {
            int ownerId = GetOwnerId(owner);
            ScratchTweens.Clear();
            for (int i = 0; i < ActiveTweens.Count; i++)
            {
                ActiveTween tween = ActiveTweens[i];
                if (tween.OwnerId == ownerId)
                {
                    ScratchTweens.Add(tween);
                }
            }

            for (int i = 0; i < ScratchTweens.Count; i++)
            {
                CancelTween(ScratchTweens[i], false);
            }

            ScratchTweens.Clear();
        }

        public static void CompleteOwner(UnityEngine.Object owner)
        {
            int ownerId = GetOwnerId(owner);
            ScratchTweens.Clear();
            for (int i = 0; i < ActiveTweens.Count; i++)
            {
                ActiveTween tween = ActiveTweens[i];
                if (tween.OwnerId == ownerId)
                {
                    ScratchTweens.Add(tween);
                }
            }

            for (int i = 0; i < ScratchTweens.Count; i++)
            {
                CancelTween(ScratchTweens[i], true);
            }

            ScratchTweens.Clear();
        }

        public static void ApplyStateEnd(RectTransform target, UIAnimationState state)
        {
            if (target == null || state == null)
            {
                return;
            }

            state.EnsureTypes();
            CanvasGroup canvasGroup = NeedsCanvasGroup(state) ? EnsureCanvasGroup(target) : target.GetComponent<CanvasGroup>();
            StartValues startValues = GetStartValues(target, canvasGroup);
            ApplyPropertyEnd(target, canvasGroup, startValues, state.move, UIAnimationType.Move);
            ApplyPropertyEnd(target, canvasGroup, startValues, state.rotate, UIAnimationType.Rotate);
            ApplyPropertyEnd(target, canvasGroup, startValues, state.scale, UIAnimationType.Scale);
            ApplyPropertyEnd(target, canvasGroup, startValues, state.fade, UIAnimationType.Fade);
        }

        public static void CaptureStart(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            StartValuesByTarget[target.GetInstanceID()] = StartValues.Capture(target, canvasGroup);
        }

        /// <summary>
        /// Restores captured start transform/alpha so a following Show
        /// (e.g. scale to CurrentValue) does not animate 0→0 after Hide.
        /// </summary>
        public static void ApplyStartValues(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            StartValues startValues = GetStartValues(target, canvasGroup);
            target.anchoredPosition3D = startValues.AnchoredPosition3D;
            target.localEulerAngles = startValues.LocalEulerAngles;
            target.localScale = startValues.LocalScale;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = startValues.Alpha;
            }
        }

        private static void PlayProperty(
            UnityEngine.Object owner,
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings settings,
            UIAnimationType type,
            bool instant,
            ref int pending,
            ref bool hasAnimation,
            Action onComplete)
        {
            if (settings == null || !settings.enabled)
            {
                return;
            }

            settings.SetType(type);
            hasAnimation = true;
            string key = BuildKey(target, type);
            CancelTweenByKey(key, false);

            if (type == UIAnimationType.Fade && canvasGroup == null)
            {
                canvasGroup = EnsureCanvasGroup(target);
            }

            if (instant || settings.duration <= 0f)
            {
                ApplyPropertyEnd(target, canvasGroup, startValues, settings, type);
                return;
            }

            pending++;
            ActiveTween activeTween = new ActiveTween
            {
                Key = key,
                OwnerId = GetOwnerId(owner),
                CompleteAction = onComplete
            };

            activeTween.FinalApply = delegate
            {
                ApplyPropertyEnd(target, canvasGroup, startValues, settings, type);
            };

            activeTween.Coroutine = GetRunner().StartCoroutine(TweenCoroutine(activeTween, target, canvasGroup, startValues, settings, type));
            ActiveByKey[key] = activeTween;
            ActiveTweens.Add(activeTween);
        }

        private static void PlayTransitionProperty(
            UnityEngine.Object owner,
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings targetSettings,
            UIAnimationSettings previousSettings,
            UIAnimationType type,
            bool instant,
            ref int pending,
            ref bool hasAnimation,
            Action onComplete)
        {
            // Return-to-start only if the PREVIOUS state actually animated this property.
            // Otherwise (e.g. Move never enabled on button states) a manual RectTransform edit
            // would look "different from start" and get snapped back — breaking prefab editing.
            bool hasTargetAnimation = targetSettings != null && targetSettings.enabled;
            bool hadPreviousAnimation = previousSettings != null && previousSettings.enabled;
            bool shouldReturnToStart = !hasTargetAnimation && hadPreviousAnimation;
            if (!hasTargetAnimation && !shouldReturnToStart)
            {
                return;
            }

            hasAnimation = true;
            string key = BuildKey(target, type);
            CancelTweenByKey(key, false);

            if (type == UIAnimationType.Fade && canvasGroup == null)
            {
                canvasGroup = EnsureCanvasGroup(target);
            }

            UIAnimationSettings timingSettings = hasTargetAnimation ? targetSettings : previousSettings;
            bool returnToStart = !hasTargetAnimation;

            if (instant || GetTransitionDuration(timingSettings, returnToStart) <= 0f)
            {
                ApplyTransitionPropertyEnd(target, canvasGroup, startValues, targetSettings, type, returnToStart);
                return;
            }

            pending++;
            ActiveTween activeTween = new ActiveTween
            {
                Key = key,
                OwnerId = GetOwnerId(owner),
                CompleteAction = onComplete
            };

            activeTween.FinalApply = delegate
            {
                ApplyTransitionPropertyEnd(target, canvasGroup, startValues, targetSettings, type, returnToStart);
            };

            activeTween.Coroutine = GetRunner().StartCoroutine(TweenTransitionCoroutine(activeTween, target, canvasGroup, startValues, targetSettings, timingSettings, type, returnToStart));
            ActiveByKey[key] = activeTween;
            ActiveTweens.Add(activeTween);
        }

        private static IEnumerator TweenCoroutine(ActiveTween activeTween, RectTransform target, CanvasGroup canvasGroup, StartValues startValues, UIAnimationSettings settings, UIAnimationType type)
        {
            if (settings.delay > 0f)
            {
                float wait = 0f;
                while (wait < settings.delay)
                {
                    if (target == null)
                    {
                        CancelTween(activeTween, false);
                        yield break;
                    }

                    wait += settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }
            }

            Vector3 vectorFrom = Vector3.zero;
            Vector3 vectorTo = Vector3.zero;
            float floatFrom = 0f;
            float floatTo = 0f;

            ResolveValues(target, canvasGroup, startValues, settings, type, out vectorFrom, out vectorTo, out floatFrom, out floatTo);

            int completedLoops = 0;
            bool infinite = settings.playMode != UIAnimationPlayMode.Once && settings.loopCount < 0;
            int loopTarget = settings.playMode == UIAnimationPlayMode.Once ? 1 : Mathf.Max(1, settings.loopCount);

            while (infinite || completedLoops < loopTarget)
            {
                float elapsed = 0f;
                while (elapsed < settings.duration)
                {
                    if (target == null)
                    {
                        CancelTween(activeTween, false);
                        yield break;
                    }

                    float t = settings.duration <= 0f ? 1f : elapsed / settings.duration;
                    ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, settings.Evaluate(t));
                    elapsed += settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }

                ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, 1f);
                completedLoops++;

                if (settings.playMode == UIAnimationPlayMode.PingPong)
                {
                    Vector3 vectorSwap = vectorFrom;
                    vectorFrom = vectorTo;
                    vectorTo = vectorSwap;
                    float floatSwap = floatFrom;
                    floatFrom = floatTo;
                    floatTo = floatSwap;
                }

                if ((infinite || completedLoops < loopTarget) && settings.loopDelay > 0f)
                {
                    float wait = 0f;
                    while (wait < settings.loopDelay)
                    {
                        if (target == null)
                        {
                            CancelTween(activeTween, false);
                            yield break;
                        }

                        wait += settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                        yield return null;
                    }
                }
            }

            FinishTween(activeTween, true);
        }

        private static IEnumerator TweenTransitionCoroutine(
            ActiveTween activeTween,
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings targetSettings,
            UIAnimationSettings timingSettings,
            UIAnimationType type,
            bool returnToStart)
        {
            float delay = returnToStart ? 0f : GetTransitionDelay(timingSettings);
            if (delay > 0f)
            {
                float wait = 0f;
                while (wait < delay)
                {
                    if (target == null)
                    {
                        CancelTween(activeTween, false);
                        yield break;
                    }

                    wait += GetTransitionDeltaTime(timingSettings, returnToStart);
                    yield return null;
                }
            }

            Vector3 vectorFrom;
            Vector3 vectorTo;
            float floatFrom;
            float floatTo;
            ResolveTransitionValues(target, canvasGroup, startValues, targetSettings, type, returnToStart, out vectorFrom, out vectorTo, out floatFrom, out floatTo);

            int completedLoops = 0;
            bool infinite = !returnToStart && timingSettings != null && timingSettings.playMode != UIAnimationPlayMode.Once && timingSettings.loopCount < 0;
            int loopTarget = returnToStart || timingSettings == null || timingSettings.playMode == UIAnimationPlayMode.Once ? 1 : Mathf.Max(1, timingSettings.loopCount);
            float duration = GetTransitionDuration(timingSettings, returnToStart);

            while (infinite || completedLoops < loopTarget)
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (target == null)
                    {
                        CancelTween(activeTween, false);
                        yield break;
                    }

                    float t = duration <= 0f ? 1f : elapsed / duration;
                    ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, EvaluateTransition(timingSettings, returnToStart, t));
                    elapsed += GetTransitionDeltaTime(timingSettings, returnToStart);
                    yield return null;
                }

                ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, 1f);
                completedLoops++;

                if (!returnToStart && timingSettings != null && timingSettings.playMode == UIAnimationPlayMode.PingPong)
                {
                    Vector3 vectorSwap = vectorFrom;
                    vectorFrom = vectorTo;
                    vectorTo = vectorSwap;
                    float floatSwap = floatFrom;
                    floatFrom = floatTo;
                    floatTo = floatSwap;
                }

                float loopDelay = returnToStart || timingSettings == null ? 0f : timingSettings.loopDelay;
                if ((infinite || completedLoops < loopTarget) && loopDelay > 0f)
                {
                    float wait = 0f;
                    while (wait < loopDelay)
                    {
                        if (target == null)
                        {
                            CancelTween(activeTween, false);
                            yield break;
                        }

                        wait += GetTransitionDeltaTime(timingSettings, returnToStart);
                        yield return null;
                    }
                }
            }

            FinishTween(activeTween, true);
        }

        private static void ResolveValues(
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings settings,
            UIAnimationType type,
            out Vector3 vectorFrom,
            out Vector3 vectorTo,
            out float floatFrom,
            out float floatTo)
        {
            vectorFrom = Vector3.zero;
            vectorTo = Vector3.zero;
            floatFrom = 0f;
            floatTo = 0f;

            switch (type)
            {
                case UIAnimationType.Move:
                    vectorFrom = settings.ResolveVectorFrom(target.anchoredPosition3D, startValues.AnchoredPosition3D, target);
                    vectorTo = settings.ResolveVectorTo(target.anchoredPosition3D, startValues.AnchoredPosition3D, target);
                    break;
                case UIAnimationType.Rotate:
                    vectorFrom = settings.ResolveVectorFrom(target.localEulerAngles, startValues.LocalEulerAngles, target);
                    vectorTo = settings.ResolveVectorTo(target.localEulerAngles, startValues.LocalEulerAngles, target);
                    break;
                case UIAnimationType.Scale:
                    vectorFrom = settings.ResolveVectorFrom(target.localScale, startValues.LocalScale, target);
                    vectorTo = settings.ResolveVectorTo(target.localScale, startValues.LocalScale, target);
                    break;
                case UIAnimationType.Fade:
                    float currentAlpha = canvasGroup == null ? 1f : canvasGroup.alpha;
                    floatFrom = settings.ResolveFloatFrom(currentAlpha, startValues.Alpha);
                    floatTo = settings.ResolveFloatTo(currentAlpha, startValues.Alpha);
                    break;
            }
        }

        private static void ResolveTransitionValues(
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings targetSettings,
            UIAnimationType type,
            bool returnToStart,
            out Vector3 vectorFrom,
            out Vector3 vectorTo,
            out float floatFrom,
            out float floatTo)
        {
            vectorFrom = Vector3.zero;
            vectorTo = Vector3.zero;
            floatFrom = 0f;
            floatTo = 0f;

            switch (type)
            {
                case UIAnimationType.Move:
                    vectorFrom = target.anchoredPosition3D;
                    vectorTo = returnToStart || targetSettings == null
                        ? startValues.AnchoredPosition3D
                        : targetSettings.ResolveVectorTo(target.anchoredPosition3D, startValues.AnchoredPosition3D, target);
                    break;
                case UIAnimationType.Rotate:
                    vectorFrom = target.localEulerAngles;
                    vectorTo = returnToStart || targetSettings == null
                        ? startValues.LocalEulerAngles
                        : targetSettings.ResolveVectorTo(target.localEulerAngles, startValues.LocalEulerAngles, target);
                    break;
                case UIAnimationType.Scale:
                    vectorFrom = target.localScale;
                    vectorTo = returnToStart || targetSettings == null
                        ? startValues.LocalScale
                        : targetSettings.ResolveVectorTo(target.localScale, startValues.LocalScale, target);
                    break;
                case UIAnimationType.Fade:
                    float currentAlpha = canvasGroup == null ? 1f : canvasGroup.alpha;
                    floatFrom = currentAlpha;
                    floatTo = returnToStart || targetSettings == null
                        ? startValues.Alpha
                        : targetSettings.ResolveFloatTo(currentAlpha, startValues.Alpha);
                    break;
            }
        }

        private static void ApplyPropertyEnd(RectTransform target, CanvasGroup canvasGroup, StartValues startValues, UIAnimationSettings settings, UIAnimationType type)
        {
            if (target == null || settings == null || !settings.enabled)
            {
                return;
            }

            Vector3 vectorFrom;
            Vector3 vectorTo;
            float floatFrom;
            float floatTo;
            ResolveValues(target, canvasGroup, startValues, settings, type, out vectorFrom, out vectorTo, out floatFrom, out floatTo);
            ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, 1f);
        }

        private static void ApplyTransitionPropertyEnd(RectTransform target, CanvasGroup canvasGroup, StartValues startValues, UIAnimationSettings targetSettings, UIAnimationType type, bool returnToStart)
        {
            if (target == null)
            {
                return;
            }

            Vector3 vectorFrom;
            Vector3 vectorTo;
            float floatFrom;
            float floatTo;
            ResolveTransitionValues(target, canvasGroup, startValues, targetSettings, type, returnToStart, out vectorFrom, out vectorTo, out floatFrom, out floatTo);
            ApplyInterpolated(target, canvasGroup, type, vectorFrom, vectorTo, floatFrom, floatTo, 1f);
        }

        private static void ApplyInterpolated(RectTransform target, CanvasGroup canvasGroup, UIAnimationType type, Vector3 vectorFrom, Vector3 vectorTo, float floatFrom, float floatTo, float t)
        {
            if (target == null)
            {
                return;
            }

            switch (type)
            {
                case UIAnimationType.Move:
                    target.anchoredPosition3D = Vector3.LerpUnclamped(vectorFrom, vectorTo, t);
                    break;
                case UIAnimationType.Rotate:
                    target.localEulerAngles = Vector3.LerpUnclamped(vectorFrom, vectorTo, t);
                    break;
                case UIAnimationType.Scale:
                    target.localScale = Vector3.LerpUnclamped(vectorFrom, vectorTo, t);
                    break;
                case UIAnimationType.Fade:
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = Mathf.LerpUnclamped(floatFrom, floatTo, t);
                    }
                    break;
            }
        }

        private static bool NeedsCanvasGroup(UIAnimationState state)
        {
            return state != null && state.fade != null && state.fade.enabled;
        }

        private static float GetTransitionDelay(UIAnimationSettings timingSettings)
        {
            return timingSettings == null ? 0f : Mathf.Max(0f, timingSettings.delay);
        }

        private static float GetTransitionDuration(UIAnimationSettings timingSettings, bool returnToStart)
        {
            if (timingSettings == null)
            {
                return 0.2f;
            }

            return Mathf.Max(0f, timingSettings.duration);
        }

        private static float GetTransitionDeltaTime(UIAnimationSettings timingSettings, bool returnToStart)
        {
            if (timingSettings == null || timingSettings.useUnscaledTime)
            {
                return Time.unscaledDeltaTime;
            }

            return Time.deltaTime;
        }

        private static float EvaluateTransition(UIAnimationSettings timingSettings, bool returnToStart, float normalizedTime)
        {
            if (timingSettings != null)
            {
                return timingSettings.Evaluate(normalizedTime);
            }

            float t = Mathf.Clamp01(normalizedTime);
            return t * (2f - t);
        }

        private static CanvasGroup EnsureCanvasGroup(RectTransform target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }

        private static StartValues GetStartValues(RectTransform target, CanvasGroup canvasGroup)
        {
            int id = target.GetInstanceID();
            StartValues values;
            if (!StartValuesByTarget.TryGetValue(id, out values))
            {
                values = StartValues.Capture(target, canvasGroup);
                StartValuesByTarget[id] = values;
            }

            return values;
        }

        private static UIAnimationTweenRunner GetRunner()
        {
            if (runner != null)
            {
                return runner;
            }

            GameObject runnerObject = new GameObject(RunnerName);
            runnerObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(runnerObject);
            runner = runnerObject.AddComponent<UIAnimationTweenRunner>();
            return runner;
        }

        private static void CancelTweenByKey(string key, bool complete)
        {
            ActiveTween activeTween;
            if (ActiveByKey.TryGetValue(key, out activeTween))
            {
                CancelTween(activeTween, complete);
            }
        }

        private static void CancelTween(ActiveTween activeTween, bool complete)
        {
            if (activeTween == null || activeTween.IsFinished)
            {
                return;
            }

            if (complete && activeTween.FinalApply != null)
            {
                activeTween.FinalApply.Invoke();
            }

            if (runner != null && activeTween.Coroutine != null)
            {
                runner.StopCoroutine(activeTween.Coroutine);
            }

            FinishTween(activeTween, false);
        }

        private static void FinishTween(ActiveTween activeTween, bool invokeComplete)
        {
            if (activeTween == null || activeTween.IsFinished)
            {
                return;
            }

            activeTween.IsFinished = true;
            ActiveByKey.Remove(activeTween.Key);
            ActiveTweens.Remove(activeTween);

            if (invokeComplete && activeTween.CompleteAction != null)
            {
                activeTween.CompleteAction.Invoke();
            }
        }

        private static string BuildKey(RectTransform target, UIAnimationType type)
        {
            return target.GetHashCode().ToString() + ":" + type;
        }

        private static int GetOwnerId(UnityEngine.Object owner)
        {
            return owner == null ? 0 : owner.GetHashCode();
        }

        private sealed class ActiveTween
        {
            public string Key;
            public int OwnerId;
            public Coroutine Coroutine;
            public Action CompleteAction;
            public Action FinalApply;
            public bool IsFinished;
        }

        private struct StartValues
        {
            public Vector3 AnchoredPosition3D;
            public Vector3 LocalEulerAngles;
            public Vector3 LocalScale;
            public float Alpha;

            public static StartValues Capture(RectTransform target, CanvasGroup canvasGroup)
            {
                StartValues values;
                values.AnchoredPosition3D = target.anchoredPosition3D;
                values.LocalEulerAngles = target.localEulerAngles;
                values.LocalScale = target.localScale;
                values.Alpha = canvasGroup == null ? 1f : canvasGroup.alpha;
                return values;
            }
        }
    }

    internal sealed class UIAnimationTweenRunner : MonoBehaviour
    {
    }
}



