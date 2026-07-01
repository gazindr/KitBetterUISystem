#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.UI
{
    public static class UIEditorDelay
    {
        private static readonly List<DelayRequest> Requests = new List<DelayRequest>(16);

        public static void Run(float delay, Action action)
        {
            if (action == null)
            {
                return;
            }

            Requests.Add(new DelayRequest
            {
                executeAt = EditorApplication.timeSinceStartup + Mathf.Max(0f, delay),
                action = action
            });

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            for (int i = Requests.Count - 1; i >= 0; i--)
            {
                if (now < Requests[i].executeAt)
                {
                    continue;
                }

                Action action = Requests[i].action;
                Requests.RemoveAt(i);
                action.Invoke();
            }

            if (Requests.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }

        private struct DelayRequest
        {
            public double executeAt;
            public Action action;
        }
    }
}
#endif
