using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    public static class UIInteractionQueueManager
    {
        private const string RunnerName = "[UI System Interaction Queue]";
        private static readonly Dictionary<string, QueueState> QueueStates = new Dictionary<string, QueueState>();
        private static UIInteractionQueueRunner runner;

        public static void Enqueue(MonoBehaviour host, string group, Action action, float releaseDelay)
        {
            if (action == null)
            {
                return;
            }

            QueueState state = GetState(group);
            QueueRequest request = new QueueRequest
            {
                host = host,
                action = action,
                releaseDelay = Mathf.Max(0f, releaseDelay),
                group = NormalizeGroup(group)
            };

            if (!state.active)
            {
                RunRequest(state, request);
                return;
            }

            state.queue.Enqueue(request);
        }

        public static void Clear()
        {
            QueueStates.Clear();
        }

        private static QueueState GetState(string group)
        {
            group = NormalizeGroup(group);
            QueueState state;
            if (!QueueStates.TryGetValue(group, out state))
            {
                state = new QueueState();
                QueueStates[group] = state;
            }

            return state;
        }

        private static void RunRequest(QueueState state, QueueRequest request)
        {
            state.active = true;
            request.action.Invoke();

            MonoBehaviour coroutineHost = request.host != null && request.host.gameObject.activeInHierarchy
                ? request.host
                : GetRunner();

            coroutineHost.StartCoroutine(ReleaseAfter(request.group, request.releaseDelay));
        }

        private static IEnumerator ReleaseAfter(string group, float delay)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Release(group);
        }

        private static void Release(string group)
        {
            QueueState state = GetState(group);
            state.active = false;

            while (!state.active && state.queue.Count > 0)
            {
                QueueRequest next = state.queue.Dequeue();
                if (next.action != null)
                {
                    RunRequest(state, next);
                    break;
                }
            }
        }

        private static UIInteractionQueueRunner GetRunner()
        {
            if (runner != null)
            {
                return runner;
            }

            GameObject runnerObject = new GameObject(RunnerName);
            runnerObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(runnerObject);
            runner = runnerObject.AddComponent<UIInteractionQueueRunner>();
            return runner;
        }

        private static string NormalizeGroup(string group)
        {
            return string.IsNullOrEmpty(group) ? "Default" : group;
        }

        private sealed class QueueState
        {
            public bool active;
            public Queue<QueueRequest> queue = new Queue<QueueRequest>();
        }

        private struct QueueRequest
        {
            public MonoBehaviour host;
            public Action action;
            public float releaseDelay;
            public string group;
        }
    }

    internal sealed class UIInteractionQueueRunner : MonoBehaviour
    {
    }
}



