using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    public static class UIContainerQueueManager
    {
        private const string RunnerName = "[UI System Container Queue]";
        private static readonly Dictionary<string, QueueState> QueueStates = new Dictionary<string, QueueState>();
        private static UIContainerQueueRunner runner;

        public static bool RequestShow(UIContainer container)
        {
            if (container == null || !container.UseInQueue)
            {
                return false;
            }

            QueueState state = GetState(container.QueueGroup);
            if (state.active == container)
            {
                return state.pendingShow != null;
            }

            if (state.active == null || (state.pendingShow == null && state.active.State == UIContainerState.Hidden))
            {
                StopPendingShow(state);
                state.active = container;
                return false;
            }

            if (!Contains(state.queue, container))
            {
                state.queue.Enqueue(container);
            }

            return true;
        }

        public static void NotifyHidden(UIContainer container)
        {
            if (container == null || !container.UseInQueue)
            {
                return;
            }

            QueueState state = GetState(container.QueueGroup);
            if (state.active == container)
            {
                StopPendingShow(state);
                state.active = null;
            }

            TryStartNext(state);
        }

        public static void Remove(UIContainer container)
        {
            if (container == null || !container.UseInQueue)
            {
                return;
            }

            QueueState state = GetState(container.QueueGroup);
            if (state.active == container)
            {
                StopPendingShow(state);
                state.active = null;
            }

            if (Contains(state.queue, container))
            {
                Queue<UIContainer> rebuilt = new Queue<UIContainer>();
                while (state.queue.Count > 0)
                {
                    UIContainer queued = state.queue.Dequeue();
                    if (queued != container)
                    {
                        rebuilt.Enqueue(queued);
                    }
                }

                state.queue = rebuilt;
            }

            TryStartNext(state);
        }

        public static void Clear()
        {
            foreach (KeyValuePair<string, QueueState> pair in QueueStates)
            {
                StopPendingShow(pair.Value);
            }

            QueueStates.Clear();
        }

        private static QueueState GetState(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                group = "Default";
            }

            QueueState state;
            if (!QueueStates.TryGetValue(group, out state))
            {
                state = new QueueState();
                QueueStates[group] = state;
            }

            return state;
        }

        private static void TryStartNext(QueueState state)
        {
            while (state.active == null && state.queue.Count > 0)
            {
                UIContainer next = state.queue.Dequeue();
                if (next == null)
                {
                    continue;
                }

                state.active = next;
                float delay = next.QueueShowDelay;
                if (delay > 0f)
                {
                    state.pendingShow = GetRunner().StartCoroutine(ShowAfterDelay(state, next, delay));
                }
                else
                {
                    next.ShowFromQueue();
                }

                break;
            }
        }

        private static IEnumerator ShowAfterDelay(QueueState state, UIContainer container, float delay)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (state.active != container || container == null)
                {
                    state.pendingShow = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            state.pendingShow = null;
            if (state.active == container && container != null)
            {
                container.ShowFromQueue();
            }
        }

        private static void StopPendingShow(QueueState state)
        {
            if (state == null || state.pendingShow == null)
            {
                return;
            }

            UIContainerQueueRunner queueRunner = runner;
            if (queueRunner != null)
            {
                queueRunner.StopCoroutine(state.pendingShow);
            }

            state.pendingShow = null;
        }

        private static UIContainerQueueRunner GetRunner()
        {
            if (runner != null)
            {
                return runner;
            }

            GameObject runnerObject = new GameObject(RunnerName);
            runnerObject.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(runnerObject);
            runner = runnerObject.AddComponent<UIContainerQueueRunner>();
            return runner;
        }

        private static bool Contains(Queue<UIContainer> queue, UIContainer container)
        {
            foreach (UIContainer queued in queue)
            {
                if (queued == container)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class QueueState
        {
            public UIContainer active;
            public Queue<UIContainer> queue = new Queue<UIContainer>();
            public Coroutine pendingShow;
        }
    }

    internal sealed class UIContainerQueueRunner : MonoBehaviour
    {
    }
}
