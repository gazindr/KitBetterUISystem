using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.UI
{
    [Serializable]
    public sealed class UIBehaviourEntry
    {
        [HorizontalGroup("Header")]
        [LabelWidth(46)]
        [HideInInspector]
        public string name = "Entry";

        [HorizontalGroup("Header", Width = 70)]
        [LabelWidth(50)]
        public bool enabled = true;

        [MinValue(0f)]
        public float delay;

        public bool executeOnce;

        [LabelText("Log Execution")]
        [Tooltip("Writes one Console log when this behaviour entry executes.")]
        public bool debugLogging;

        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        [LabelText("Target Container Override")]
        [Tooltip("Optional. Show/Hide/Toggle Container actions use this container when their own target is empty.")]
        public UIContainer targetContainer;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourAction> actions = new List<UIBehaviourAction>();

        [FoldoutGroup("Callback")]
        public UnityEvent callback = new UnityEvent();

        [NonSerialized]
        private bool executed;

        public void Execute(UIBehaviourContext context, MonoBehaviour coroutineHost)
        {
            if (!enabled)
            {
                return;
            }

            if (executeOnce && executed)
            {
                return;
            }

            executed = true;

            if (delay > 0f && coroutineHost != null && coroutineHost.gameObject.activeInHierarchy)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UIEditorDelay.Run(delay, delegate { ExecuteNow(context); });
                    return;
                }
#endif
                coroutineHost.StartCoroutine(ExecuteDelayed(context, delay));
                return;
            }

            ExecuteNow(context);
        }

        public void ResetRuntimeState()
        {
            executed = false;
        }

        public float GetEstimatedDuration()
        {
            return enabled ? Mathf.Max(0f, delay) : 0f;
        }

        public UIBehaviourEntry Clone(bool includeCallbacks)
        {
            UIBehaviourEntry clone = new UIBehaviourEntry();
            clone.CopyFrom(this, includeCallbacks);
            return clone;
        }

        public void CopyFrom(UIBehaviourEntry source, bool includeCallbacks)
        {
            if (source == null)
            {
                return;
            }

            name = source.name;
            enabled = source.enabled;
            delay = source.delay;
            executeOnce = source.executeOnce;
            debugLogging = source.debugLogging;
            targetContainer = source.targetContainer;
            actions = source.actions == null ? new List<UIBehaviourAction>() : new List<UIBehaviourAction>(source.actions);
            callback = includeCallbacks && source.callback != null ? source.callback : new UnityEvent();
            executed = false;
        }

        private IEnumerator ExecuteDelayed(UIBehaviourContext context, float waitTime)
        {
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ExecuteNow(context);
        }

        private void ExecuteNow(UIBehaviourContext context)
        {
            if (context == null)
            {
                return;
            }

            UIContainer previousTarget = context.targetContainer;
            if (targetContainer != null)
            {
                context.targetContainer = targetContainer;
            }

            if (debugLogging)
            {
                Debug.Log("[UISystem] Behaviour entry '" + name + "' executed by " + (context.sourceGameObject == null ? "Unknown" : context.sourceGameObject.name) + " on " + context.trigger);
            }

            if (callback != null)
            {
                callback.Invoke();
            }

            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    UIBehaviourAction action = actions[i];
                    if (action != null)
                    {
                        action.Execute(context);
                    }
                }
            }

            context.targetContainer = previousTarget;
        }
    }
}



