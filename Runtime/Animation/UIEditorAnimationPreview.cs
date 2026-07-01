#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.UI
{
    public static class UIEditorAnimationPreview
    {
        private static readonly Dictionary<string, PreviewTween> ActiveByKey = new Dictionary<string, PreviewTween>();
        private static readonly List<PreviewTween> ActiveTweens = new List<PreviewTween>(32);
        private static readonly List<PreviewTween> ScratchTweens = new List<PreviewTween>(32);

        public static void PlayState(UnityEngine.Object owner, RectTransform target, UIAnimationState state, Action onComplete)
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
            StartValues startValues = StartValues.Capture(target, canvasGroup);

            int pending = CountEnabledAnimations(state);
            if (pending <= 0)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            Action propertyComplete = delegate
            {
                pending--;
                if (pending <= 0 && onComplete != null)
                {
                    onComplete.Invoke();
                }
            };

            AddTween(owner, target, canvasGroup, startValues, state.move, UIAnimationType.Move, propertyComplete);
            AddTween(owner, target, canvasGroup, startValues, state.rotate, UIAnimationType.Rotate, propertyComplete);
            AddTween(owner, target, canvasGroup, startValues, state.scale, UIAnimationType.Scale, propertyComplete);
            AddTween(owner, target, canvasGroup, startValues, state.fade, UIAnimationType.Fade, propertyComplete);
        }

        public static void StopOwner(UnityEngine.Object owner)
        {
            int ownerId = GetOwnerId(owner);
            ScratchTweens.Clear();
            for (int i = 0; i < ActiveTweens.Count; i++)
            {
                if (ActiveTweens[i].ownerId == ownerId)
                {
                    ScratchTweens.Add(ActiveTweens[i]);
                }
            }

            for (int i = 0; i < ScratchTweens.Count; i++)
            {
                RemoveTween(ScratchTweens[i], false);
            }

            ScratchTweens.Clear();
        }

        public static void CompleteOwner(UnityEngine.Object owner)
        {
            int ownerId = GetOwnerId(owner);
            ScratchTweens.Clear();
            for (int i = 0; i < ActiveTweens.Count; i++)
            {
                if (ActiveTweens[i].ownerId == ownerId)
                {
                    ScratchTweens.Add(ActiveTweens[i]);
                }
            }

            for (int i = 0; i < ScratchTweens.Count; i++)
            {
                ApplyTween(ScratchTweens[i], 1f);
                RemoveTween(ScratchTweens[i], true);
            }

            ScratchTweens.Clear();
        }

        private static void AddTween(
            UnityEngine.Object owner,
            RectTransform target,
            CanvasGroup canvasGroup,
            StartValues startValues,
            UIAnimationSettings settings,
            UIAnimationType type,
            Action onComplete)
        {
            if (settings == null || !settings.enabled)
            {
                return;
            }

            settings.SetType(type);

            if (type == UIAnimationType.Fade && canvasGroup == null)
            {
                canvasGroup = EnsureCanvasGroup(target);
            }

            string key = BuildKey(target, type);
            RemoveTweenByKey(key, false);

            PreviewTween tween = new PreviewTween();
            tween.key = key;
            tween.ownerId = GetOwnerId(owner);
            tween.target = target;
            tween.canvasGroup = canvasGroup;
            tween.settings = settings;
            tween.type = type;
            tween.startTime = EditorApplication.timeSinceStartup;
            tween.onComplete = onComplete;
            tween.loopCount = settings.playMode == UIAnimationPlayMode.Once ? 1 : Mathf.Max(1, settings.loopCount);
            if (settings.playMode != UIAnimationPlayMode.Once && settings.loopCount < 0)
            {
                tween.loopCount = 1;
            }

            ResolveValues(target, canvasGroup, startValues, settings, type, out tween.vectorFrom, out tween.vectorTo, out tween.floatFrom, out tween.floatTo);
            ApplyTween(tween, 0f);

            if (settings.duration <= 0f)
            {
                ApplyTween(tween, 1f);
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }

                return;
            }

            ActiveByKey[key] = tween;
            ActiveTweens.Add(tween);
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            ScratchTweens.Clear();

            for (int i = 0; i < ActiveTweens.Count; i++)
            {
                PreviewTween tween = ActiveTweens[i];
                if (tween.target == null || tween.settings == null)
                {
                    ScratchTweens.Add(tween);
                    continue;
                }

                double elapsedTotal = now - tween.startTime;
                if (elapsedTotal < tween.settings.delay)
                {
                    continue;
                }

                float duration = Mathf.Max(0.0001f, tween.settings.duration);
                float loopBlock = duration + Mathf.Max(0f, tween.settings.loopDelay);
                float elapsedAfterDelay = (float)(elapsedTotal - tween.settings.delay);
                int currentLoop = Mathf.FloorToInt(elapsedAfterDelay / loopBlock);
                float elapsedInLoop = elapsedAfterDelay - currentLoop * loopBlock;

                if (currentLoop >= tween.loopCount)
                {
                    ApplyTween(tween, 1f);
                    ScratchTweens.Add(tween);
                    continue;
                }

                if (elapsedInLoop > duration)
                {
                    ApplyTween(tween, tween.settings.playMode == UIAnimationPlayMode.PingPong && currentLoop % 2 == 1 ? 0f : 1f);
                    continue;
                }

                float normalized = Mathf.Clamp01(elapsedInLoop / duration);
                if (tween.settings.playMode == UIAnimationPlayMode.PingPong && currentLoop % 2 == 1)
                {
                    normalized = 1f - normalized;
                }

                ApplyTween(tween, tween.settings.Evaluate(normalized));
            }

            for (int i = 0; i < ScratchTweens.Count; i++)
            {
                RemoveTween(ScratchTweens[i], true);
            }

            ScratchTweens.Clear();

            if (ActiveTweens.Count == 0)
            {
                EditorApplication.update -= Update;
            }
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
                    vectorFrom = settings.ResolveVectorFrom(target.anchoredPosition3D, startValues.anchoredPosition3D);
                    vectorTo = settings.ResolveVectorTo(target.anchoredPosition3D, startValues.anchoredPosition3D);
                    break;
                case UIAnimationType.Rotate:
                    vectorFrom = settings.ResolveVectorFrom(target.localEulerAngles, startValues.localEulerAngles);
                    vectorTo = settings.ResolveVectorTo(target.localEulerAngles, startValues.localEulerAngles);
                    break;
                case UIAnimationType.Scale:
                    vectorFrom = settings.ResolveVectorFrom(target.localScale, startValues.localScale);
                    vectorTo = settings.ResolveVectorTo(target.localScale, startValues.localScale);
                    break;
                case UIAnimationType.Fade:
                    float currentAlpha = canvasGroup == null ? 1f : canvasGroup.alpha;
                    floatFrom = settings.ResolveFloatFrom(currentAlpha, startValues.alpha);
                    floatTo = settings.ResolveFloatTo(currentAlpha, startValues.alpha);
                    break;
            }
        }

        private static void ApplyTween(PreviewTween tween, float eased)
        {
            if (tween == null || tween.target == null)
            {
                return;
            }

            switch (tween.type)
            {
                case UIAnimationType.Move:
                    tween.target.anchoredPosition3D = Vector3.LerpUnclamped(tween.vectorFrom, tween.vectorTo, eased);
                    break;
                case UIAnimationType.Rotate:
                    tween.target.localEulerAngles = Vector3.LerpUnclamped(tween.vectorFrom, tween.vectorTo, eased);
                    break;
                case UIAnimationType.Scale:
                    tween.target.localScale = Vector3.LerpUnclamped(tween.vectorFrom, tween.vectorTo, eased);
                    break;
                case UIAnimationType.Fade:
                    if (tween.canvasGroup != null)
                    {
                        tween.canvasGroup.alpha = Mathf.LerpUnclamped(tween.floatFrom, tween.floatTo, eased);
                        EditorUtility.SetDirty(tween.canvasGroup);
                    }
                    break;
            }

            EditorUtility.SetDirty(tween.target);
            MarkSceneDirty(tween.target.gameObject.scene);
            SceneView.RepaintAll();
        }

        private static void RemoveTweenByKey(string key, bool invokeComplete)
        {
            PreviewTween tween;
            if (ActiveByKey.TryGetValue(key, out tween))
            {
                RemoveTween(tween, invokeComplete);
            }
        }

        private static void RemoveTween(PreviewTween tween, bool invokeComplete)
        {
            if (tween == null)
            {
                return;
            }

            ActiveByKey.Remove(tween.key);
            ActiveTweens.Remove(tween);

            if (invokeComplete && tween.onComplete != null)
            {
                tween.onComplete.Invoke();
            }
        }

        private static bool NeedsCanvasGroup(UIAnimationState state)
        {
            return state != null && state.fade != null && state.fade.enabled;
        }

        private static int CountEnabledAnimations(UIAnimationState state)
        {
            int count = 0;
            if (state.move != null && state.move.enabled)
            {
                count++;
            }

            if (state.rotate != null && state.rotate.enabled)
            {
                count++;
            }

            if (state.scale != null && state.scale.enabled)
            {
                count++;
            }

            if (state.fade != null && state.fade.enabled)
            {
                count++;
            }

            return count;
        }

        private static CanvasGroup EnsureCanvasGroup(RectTransform target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = Undo.AddComponent<CanvasGroup>(target.gameObject);
            }

            return canvasGroup;
        }

        private static string BuildKey(RectTransform target, UIAnimationType type)
        {
            return target.GetHashCode().ToString() + ":" + type;
        }

        private static int GetOwnerId(UnityEngine.Object owner)
        {
            return owner == null ? 0 : owner.GetHashCode();
        }

        private static void MarkSceneDirty(Scene scene)
        {
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        private sealed class PreviewTween
        {
            public string key;
            public int ownerId;
            public RectTransform target;
            public CanvasGroup canvasGroup;
            public UIAnimationSettings settings;
            public UIAnimationType type;
            public double startTime;
            public Action onComplete;
            public int loopCount;
            public Vector3 vectorFrom;
            public Vector3 vectorTo;
            public float floatFrom;
            public float floatTo;
        }

        private struct StartValues
        {
            public Vector3 anchoredPosition3D;
            public Vector3 localEulerAngles;
            public Vector3 localScale;
            public float alpha;

            public static StartValues Capture(RectTransform target, CanvasGroup canvasGroup)
            {
                StartValues values;
                values.anchoredPosition3D = target.anchoredPosition3D;
                values.localEulerAngles = target.localEulerAngles;
                values.localScale = target.localScale;
                values.alpha = canvasGroup == null ? 1f : canvasGroup.alpha;
                return values;
            }
        }
    }
}
#endif



