using System.Collections.Generic;
using Project.UI;
using UnityEditor;
using UnityEngine;

namespace Project.UI.Editor
{
    /// <summary>
    /// When a full preset asset changes, push non-overridden fields to all scene/prefab instances using it.
    /// </summary>
    [InitializeOnLoad]
    public static class UIPresetChangeWatcher
    {
        private static readonly HashSet<string> PendingPresetPaths = new HashSet<string>();
        private static double nextFlushTime;

        static UIPresetChangeWatcher()
        {
            Undo.postprocessModifications += OnPostprocessModifications;
            EditorApplication.update += OnEditorUpdate;
        }

        private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            if (modifications == null)
            {
                return modifications;
            }

            for (int i = 0; i < modifications.Length; i++)
            {
                Object target = modifications[i].currentValue != null
                    ? modifications[i].currentValue.target
                    : null;
                if (target is UIContainerPreset || target is UIButtonPreset)
                {
                    string path = AssetDatabase.GetAssetPath(target);
                    if (!string.IsNullOrEmpty(path))
                    {
                        PendingPresetPaths.Add(path);
                        nextFlushTime = EditorApplication.timeSinceStartup + 0.15d;
                    }
                }
            }

            return modifications;
        }

        private static void OnEditorUpdate()
        {
            if (PendingPresetPaths.Count == 0 || EditorApplication.timeSinceStartup < nextFlushTime)
            {
                return;
            }

            string[] paths = new string[PendingPresetPaths.Count];
            PendingPresetPaths.CopyTo(paths);
            PendingPresetPaths.Clear();

            for (int i = 0; i < paths.Length; i++)
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(paths[i]);
                if (asset is UIContainerPreset containerPreset)
                {
                    SyncContainerPreset(containerPreset);
                }
                else if (asset is UIButtonPreset buttonPreset)
                {
                    SyncButtonPreset(buttonPreset);
                }
            }
        }

        private static void SyncContainerPreset(UIContainerPreset preset)
        {
            UIContainer[] containers = Object.FindObjectsByType<UIContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < containers.Length; i++)
            {
                UIContainer container = containers[i];
                if (container == null || container.preset != preset)
                {
                    continue;
                }

                Undo.RecordObject(container, "Sync Container From Preset");
                container.ApplyPresetKeepingOverrides();
                EditorUtility.SetDirty(container);
            }
        }

        private static void SyncButtonPreset(UIButtonPreset preset)
        {
            UIButton[] buttons = Object.FindObjectsByType<UIButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                UIButton button = buttons[i];
                if (button == null || button.preset != preset)
                {
                    continue;
                }

                Undo.RecordObject(button, "Sync Button From Preset");
                button.ApplyPresetKeepingOverrides();
                EditorUtility.SetDirty(button);
            }
        }
    }
}
