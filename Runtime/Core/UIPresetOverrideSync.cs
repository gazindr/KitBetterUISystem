using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.UI
{
    /// <summary>
    /// Applies a full preset while restoring overridden property paths.
    /// </summary>
    public static class UIPresetOverrideSync
    {
        public static void ApplyNonOverridden(Object component, Object preset, IList<string> overriddenPaths)
        {
#if UNITY_EDITOR
            if (component == null || preset == null)
            {
                return;
            }

            SerializedObject componentSo = new SerializedObject(component);
            componentSo.Update();

            Dictionary<string, object> saved = new Dictionary<string, object>();
            if (overriddenPaths != null)
            {
                for (int i = 0; i < overriddenPaths.Count; i++)
                {
                    string path = overriddenPaths[i];
                    if (string.IsNullOrEmpty(path) || saved.ContainsKey(path))
                    {
                        continue;
                    }

                    SerializedProperty property = componentSo.FindProperty(path);
                    if (property != null)
                    {
                        saved[path] = property.boxedValue;
                    }
                }
            }

            if (component is UIContainer container && preset is UIContainerPreset containerPreset)
            {
                container.ApplyContainerPresetData(containerPreset, false);
            }
            else if (component is UIButton button && preset is UIButtonPreset buttonPreset)
            {
                button.ApplyButtonPresetData(buttonPreset, false);
            }
            else if (component is UIToggle toggle && preset is UITogglePreset togglePreset)
            {
                toggle.ApplyTogglePresetData(togglePreset, false);
            }

            componentSo.Update();
            foreach (KeyValuePair<string, object> pair in saved)
            {
                SerializedProperty property = componentSo.FindProperty(pair.Key);
                if (property != null)
                {
                    property.boxedValue = pair.Value;
                }
            }

            componentSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(component);
#endif
        }
    }
}
