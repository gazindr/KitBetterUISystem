using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Shared helpers for full-preset apply and per-field override tracking.
    /// </summary>
    public static class UIPresetOverrideUtility
    {
        public static readonly Color OverrideTint = new Color(1f, 0.78f, 0.45f, 1f);

        public static bool IsOverridden(IList<string> overriddenPaths, string path)
        {
            if (overriddenPaths == null || string.IsNullOrEmpty(path))
            {
                return false;
            }

            for (int i = 0; i < overriddenPaths.Count; i++)
            {
                if (overriddenPaths[i] == path)
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetOverride(List<string> overriddenPaths, string path, bool isOverride)
        {
            if (overriddenPaths == null || string.IsNullOrEmpty(path))
            {
                return;
            }

            int index = overriddenPaths.IndexOf(path);
            if (isOverride)
            {
                if (index < 0)
                {
                    overriddenPaths.Add(path);
                }
            }
            else if (index >= 0)
            {
                overriddenPaths.RemoveAt(index);
            }
        }

        public static void ClearOverrides(List<string> overriddenPaths)
        {
            if (overriddenPaths != null)
            {
                overriddenPaths.Clear();
            }
        }

        public static bool MatchesSerialized(object a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            return JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
        }
    }
}
