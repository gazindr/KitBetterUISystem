using System.Collections.Generic;
using Project.UI;
using UnityEditor;
using UnityEngine;

namespace Project.UI.Editor
{
    /// <summary>
    /// Presets tab bar (dirty *, Save, +) and overridable property drawing (orange + RMB Apply/Revert).
    /// </summary>
    [InitializeOnLoad]
    public static class UIPresetOverrideDrawer
    {
        private static Object activeComponent;
        private static Object activePreset;
        private static List<string> activeOverrides;
        private static SerializedObject activeComponentSo;
        private static SerializedObject activePresetSo;

        static UIPresetOverrideDrawer()
        {
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
        }

        private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property == null || property.serializedObject == null)
            {
                return;
            }

            Object target = property.serializedObject.targetObject;
            if (!(target is UIContainer) && !(target is UIButton) && !(target is UIToggle))
            {
                return;
            }

            string path = property.propertyPath;
            if (string.IsNullOrEmpty(path) ||
                path == "m_Script" ||
                path == "preset" ||
                path == "overriddenPaths" ||
                path.StartsWith("overriddenPaths.", System.StringComparison.Ordinal))
            {
                return;
            }

            Object preset = GetPreset(target);
            List<string> overrides = GetOverrides(target);

            menu.AddSeparator(string.Empty);
            if (preset == null)
            {
                menu.AddDisabledItem(new GUIContent("Apply to Preset"));
                menu.AddDisabledItem(new GUIContent("Revert to Preset"));
                return;
            }

            string capturedPath = path;
            Object capturedTarget = target;
            Object capturedPreset = preset;
            List<string> capturedOverrides = overrides;

            menu.AddItem(new GUIContent("Apply to Preset"), false, () =>
            {
                ApplyPathToPreset(capturedTarget, capturedPreset, capturedOverrides, capturedPath);
            });
            menu.AddItem(new GUIContent("Revert to Preset"), false, () =>
            {
                RevertPathFromPreset(capturedTarget, capturedPreset, capturedOverrides, capturedPath);
            });
        }

        public static void Begin(Object component, Object preset, List<string> overriddenPaths, SerializedObject componentSerializedObject)
        {
            activeComponent = component;
            activePreset = preset;
            activeOverrides = overriddenPaths;
            activeComponentSo = componentSerializedObject;
            activePresetSo = preset != null ? new SerializedObject(preset) : null;
        }

        public static void End()
        {
            activeComponent = null;
            activePreset = null;
            activeOverrides = null;
            activeComponentSo = null;
            activePresetSo = null;
        }

        public static bool IsActive
        {
            get { return activeComponent != null && activePreset != null; }
        }

        private static Object GetPreset(Object component)
        {
            UIContainer container = component as UIContainer;
            if (container != null)
            {
                return container.preset;
            }

            UIButton button = component as UIButton;
            if (button != null)
            {
                return button.preset;
            }

            UIToggle toggle = component as UIToggle;
            return toggle != null ? toggle.preset : null;
        }

        private static List<string> GetOverrides(Object component)
        {
            UIContainer container = component as UIContainer;
            if (container != null)
            {
                return container.OverriddenPaths;
            }

            UIButton button = component as UIButton;
            if (button != null)
            {
                return button.OverriddenPaths;
            }

            UIToggle toggle = component as UIToggle;
            return toggle != null ? toggle.OverriddenPaths : null;
        }

        public static void DrawContainerPresetsTab(SerializedObject serializedObject, UIContainer container)
        {
            if (container == null)
            {
                return;
            }

            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp == null)
            {
                return;
            }

            UIContainerPreset preset = presetProp.objectReferenceValue as UIContainerPreset;
            bool dirty = IsContainerDirty(container, preset);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp, new GUIContent(dirty ? "Preset *" : "Preset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                preset = presetProp.objectReferenceValue as UIContainerPreset;
                if (preset != null)
                {
                    Undo.RecordObject(container, "Apply Container Preset");
                    container.ApplyContainerPresetData(preset, true);
                    EditorUtility.SetDirty(container);
                    serializedObject.Update();
                }
                else
                {
                    UIPresetOverrideUtility.ClearOverrides(container.OverriddenPaths);
                }
            }

            using (new EditorGUI.DisabledScope(preset == null || !dirty))
            {
                if (GUILayout.Button("Save", GUILayout.Width(52f)))
                {
                    Undo.RecordObject(preset, "Save Container Preset");
                    container.SaveAllToPreset();
                    EditorUtility.SetDirty(preset);
                    EditorUtility.SetDirty(container);
                    AssetDatabase.SaveAssets();
                    serializedObject.Update();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(28f)))
            {
                CreateContainerPresetAsset(container, serializedObject);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void DrawButtonPresetsTab(SerializedObject serializedObject, UIButton button)
        {
            if (button == null)
            {
                return;
            }

            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp == null)
            {
                return;
            }

            UIButtonPreset preset = presetProp.objectReferenceValue as UIButtonPreset;
            bool dirty = IsButtonDirty(button, preset);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp, new GUIContent(dirty ? "Preset *" : "Preset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                preset = presetProp.objectReferenceValue as UIButtonPreset;
                if (preset != null)
                {
                    Undo.RecordObject(button, "Apply Button Preset");
                    button.ApplyButtonPresetData(preset, true);
                    EditorUtility.SetDirty(button);
                    serializedObject.Update();
                }
                else
                {
                    UIPresetOverrideUtility.ClearOverrides(button.OverriddenPaths);
                }
            }

            using (new EditorGUI.DisabledScope(preset == null || !dirty))
            {
                if (GUILayout.Button("Save", GUILayout.Width(52f)))
                {
                    Undo.RecordObject(preset, "Save Button Preset");
                    button.SaveAllToPreset();
                    EditorUtility.SetDirty(preset);
                    EditorUtility.SetDirty(button);
                    AssetDatabase.SaveAssets();
                    serializedObject.Update();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(28f)))
            {
                CreateButtonPresetAsset(button, serializedObject);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void DrawTogglePresetsTab(SerializedObject serializedObject, UIToggle toggle)
        {
            if (toggle == null)
            {
                return;
            }

            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp == null)
            {
                return;
            }

            UITogglePreset preset = presetProp.objectReferenceValue as UITogglePreset;
            bool dirty = IsToggleDirty(toggle, preset);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp, new GUIContent(dirty ? "Preset *" : "Preset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                preset = presetProp.objectReferenceValue as UITogglePreset;
                if (preset != null)
                {
                    Undo.RecordObject(toggle, "Apply Toggle Preset");
                    toggle.ApplyTogglePresetData(preset, true);
                    EditorUtility.SetDirty(toggle);
                    serializedObject.Update();
                }
                else
                {
                    UIPresetOverrideUtility.ClearOverrides(toggle.OverriddenPaths);
                }
            }

            using (new EditorGUI.DisabledScope(preset == null || !dirty))
            {
                if (GUILayout.Button("Save", GUILayout.Width(52f)))
                {
                    Undo.RecordObject(preset, "Save Toggle Preset");
                    toggle.SaveAllToPreset();
                    EditorUtility.SetDirty(preset);
                    EditorUtility.SetDirty(toggle);
                    AssetDatabase.SaveAssets();
                    serializedObject.Update();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(28f)))
            {
                CreateTogglePresetAsset(toggle, serializedObject);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void DrawProperty(SerializedProperty property, string label, bool includeChildren = false)
        {
            if (property == null)
            {
                return;
            }

            if (!IsActive)
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                float needed = Mathf.Clamp(EditorStyles.label.CalcSize(new GUIContent(label)).x + 18f, 150f, 220f);
                EditorGUIUtility.labelWidth = needed;
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
                EditorGUIUtility.labelWidth = previousLabelWidth;
                return;
            }

            string path = property.propertyPath;
            bool isOverride = IsPathOverridden(path);

            EditorGUI.BeginChangeCheck();
            float prevLabelWidth = EditorGUIUtility.labelWidth;
            float need = Mathf.Clamp(EditorStyles.label.CalcSize(new GUIContent(label)).x + 18f, 150f, 220f);
            EditorGUIUtility.labelWidth = need;

            Color previousBg = GUI.backgroundColor;
            Color previousContent = GUI.contentColor;
            if (isOverride)
            {
                GUI.backgroundColor = Color.Lerp(previousBg, UIPresetOverrideUtility.OverrideTint, 0.7f);
                GUI.contentColor = Color.Lerp(previousContent, new Color(1f, 0.55f, 0.15f, 1f), 0.55f);
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
            GUI.backgroundColor = previousBg;
            GUI.contentColor = previousContent;
            EditorGUIUtility.labelWidth = prevLabelWidth;

            Rect row = GUILayoutUtility.GetLastRect();
            if (isOverride && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(row.x, row.y, 3f, row.height), UIPresetOverrideUtility.OverrideTint);
                Color wash = UIPresetOverrideUtility.OverrideTint;
                wash.a = 0.18f;
                EditorGUI.DrawRect(row, wash);
            }

            bool changed = EditorGUI.EndChangeCheck();
            if (changed && activeComponentSo != null)
            {
                activeComponentSo.ApplyModifiedProperties();
                RefreshOverrideForPath(path);
                EditorUtility.SetDirty(activeComponent);
                activeComponentSo.Update();
            }
        }

        public static void DrawRelative(SerializedProperty root, string propertyName, string label, bool includeChildren = false)
        {
            if (root == null)
            {
                return;
            }

            SerializedProperty property = root.FindPropertyRelative(propertyName);
            DrawProperty(property, label, includeChildren);
        }

        private static bool IsPathOverridden(string path)
        {
            if (activeOverrides == null || string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (UIPresetOverrideUtility.IsOverridden(activeOverrides, path))
            {
                return true;
            }

            for (int i = 0; i < activeOverrides.Count; i++)
            {
                string overridden = activeOverrides[i];
                if (!string.IsNullOrEmpty(overridden) &&
                    (path.StartsWith(overridden + ".", System.StringComparison.Ordinal) ||
                     overridden.StartsWith(path + ".", System.StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RefreshOverrideForPath(string path)
        {
            if (activePresetSo == null || activeComponentSo == null || activeOverrides == null)
            {
                return;
            }

            activePresetSo.Update();
            string presetPath = MapComponentPathToPreset(path);
            SerializedProperty componentProp = activeComponentSo.FindProperty(path);
            SerializedProperty presetProp = activePresetSo.FindProperty(presetPath);
            if (componentProp == null || presetProp == null)
            {
                return;
            }

            bool equal = PropertiesEqual(componentProp, presetProp);
            UIPresetOverrideUtility.SetOverride(activeOverrides, path, !equal);
            EditorUtility.SetDirty(activeComponent);
        }

        private static bool PropertiesEqual(SerializedProperty a, SerializedProperty b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            try
            {
                return SerializedProperty.DataEquals(a, b);
            }
            catch
            {
                return Equals(a.boxedValue, b.boxedValue);
            }
        }

        private static void ApplyPathToPreset(Object component, Object preset, List<string> overrides, string path)
        {
            if (component == null || preset == null || string.IsNullOrEmpty(path))
            {
                return;
            }

            SerializedObject componentSo = new SerializedObject(component);
            SerializedObject presetSo = new SerializedObject(preset);
            componentSo.Update();
            presetSo.Update();

            string presetPath = MapComponentPathToPreset(path);
            SerializedProperty source = componentSo.FindProperty(path);
            SerializedProperty target = presetSo.FindProperty(presetPath);
            if (source == null || target == null)
            {
                Debug.LogWarning("[UI System] Apply to Preset: property not found: " + path + " → " + presetPath);
                return;
            }

            Undo.RecordObject(preset, "Apply to Preset");
            target.boxedValue = source.boxedValue;
            presetSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();

            UIPresetOverrideUtility.SetOverride(overrides, path, false);
            EditorUtility.SetDirty(component);
        }

        private static void RevertPathFromPreset(Object component, Object preset, List<string> overrides, string path)
        {
            if (component == null || preset == null || string.IsNullOrEmpty(path))
            {
                return;
            }

            SerializedObject componentSo = new SerializedObject(component);
            SerializedObject presetSo = new SerializedObject(preset);
            componentSo.Update();
            presetSo.Update();

            string presetPath = MapComponentPathToPreset(path);
            SerializedProperty source = presetSo.FindProperty(presetPath);
            SerializedProperty target = componentSo.FindProperty(path);
            if (source == null || target == null)
            {
                Debug.LogWarning("[UI System] Revert to Preset: property not found: " + path + " → " + presetPath);
                return;
            }

            Undo.RecordObject(component, "Revert to Preset");
            target.boxedValue = source.boxedValue;
            componentSo.ApplyModifiedProperties();
            UIPresetOverrideUtility.SetOverride(overrides, path, false);
            EditorUtility.SetDirty(component);
        }

        private static string MapComponentPathToPreset(string path)
        {
            if (path == "m_Interactable")
            {
                return "interactable";
            }

            return path;
        }

        private static bool IsContainerDirty(UIContainer container, UIContainerPreset preset)
        {
            if (container == null || preset == null)
            {
                return false;
            }

            if (container.OverriddenPaths != null && container.OverriddenPaths.Count > 0)
            {
                return true;
            }

            return !ValuesMatch(container, preset);
        }

        private static bool IsButtonDirty(UIButton button, UIButtonPreset preset)
        {
            if (button == null || preset == null)
            {
                return false;
            }

            if (button.OverriddenPaths != null && button.OverriddenPaths.Count > 0)
            {
                return true;
            }

            return !ValuesMatch(button, preset);
        }

        private static bool ValuesMatch(UIContainer container, UIContainerPreset preset)
        {
            if (container.category != preset.category ||
                container.autoRegister != preset.autoRegister ||
                container.registerOnAwake != preset.registerOnAwake ||
                container.startupMode != preset.startupMode ||
                container.useInQueue != preset.useInQueue ||
                container.queueGroup != preset.queueGroup ||
                !Mathf.Approximately(container.queueShowDelay, preset.queueShowDelay) ||
                container.useAutoHide != preset.useAutoHide ||
                !Mathf.Approximately(container.autoHideDelay, preset.autoHideDelay) ||
                container.deactivateOnHidden != preset.deactivateOnHidden ||
                container.muteUISound != preset.muteUISound ||
                container.customShowSound != preset.customShowSound ||
                container.customHideSound != preset.customHideSound)
            {
                return false;
            }

            return UIPresetOverrideUtility.MatchesSerialized(container.animations, preset.animations) &&
                   UIPresetOverrideUtility.MatchesSerialized(container.backgroundSettings, preset.backgroundSettings);
        }

        private static bool ValuesMatch(UIButton button, UIButtonPreset preset)
        {
            if (button.interactable != preset.interactable ||
                !Mathf.Approximately(button.doubleClickInterval, preset.doubleClickInterval) ||
                !Mathf.Approximately(button.longClickDuration, preset.longClickDuration) ||
                !Mathf.Approximately(button.clickCooldown, preset.clickCooldown) ||
                button.blockPointerWhenDisabled != preset.blockPointerWhenDisabled ||
                button.invokeOnSubmit != preset.invokeOnSubmit ||
                button.useInQueue != preset.useInQueue ||
                button.queueGroup != preset.queueGroup ||
                !Mathf.Approximately(button.queueReleaseDelay, preset.queueReleaseDelay) ||
                button.muteUISound != preset.muteUISound ||
                button.customClickSound != preset.customClickSound)
            {
                return false;
            }

            return UIPresetOverrideUtility.MatchesSerialized(button.stateAnimations, preset.stateAnimations);
        }

        private static bool IsToggleDirty(UIToggle toggle, UITogglePreset preset)
        {
            if (toggle == null || preset == null)
            {
                return false;
            }

            if (toggle.OverriddenPaths != null && toggle.OverriddenPaths.Count > 0)
            {
                return true;
            }

            return !ValuesMatch(toggle, preset);
        }

        private static bool ValuesMatch(UIToggle toggle, UITogglePreset preset)
        {
            if (toggle.interactable != preset.interactable ||
                toggle.multipleSelectCount != preset.multipleSelectCount ||
                toggle.resetMultipleCounterOnDeselect != preset.resetMultipleCounterOnDeselect ||
                toggle.blockPointerWhenDisabled != preset.blockPointerWhenDisabled ||
                toggle.invokeOnSubmit != preset.invokeOnSubmit ||
                toggle.useInQueue != preset.useInQueue ||
                toggle.queueGroup != preset.queueGroup ||
                !Mathf.Approximately(toggle.queueReleaseDelay, preset.queueReleaseDelay))
            {
                return false;
            }

            return UIPresetOverrideUtility.MatchesSerialized(toggle.stateAnimations, preset.stateAnimations) &&
                   UIPresetOverrideUtility.MatchesSerialized(toggle.backgroundSelectAnimation, preset.backgroundSelectAnimation) &&
                   UIPresetOverrideUtility.MatchesSerialized(toggle.backgroundDeselectAnimation, preset.backgroundDeselectAnimation) &&
                   UIPresetOverrideUtility.MatchesSerialized(toggle.handleSelectAnimation, preset.handleSelectAnimation) &&
                   UIPresetOverrideUtility.MatchesSerialized(toggle.handleDeselectAnimation, preset.handleDeselectAnimation);
        }

        private static void CreateContainerPresetAsset(UIContainer container, SerializedObject serializedObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Container Preset",
                "UIContainerPreset",
                "asset",
                "Choose a location for the new container preset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            UIContainerPreset asset = ScriptableObject.CreateInstance<UIContainerPreset>();
            AssetDatabase.CreateAsset(asset, path);
            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp != null)
            {
                presetProp.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }

            container.preset = asset;
            container.SaveAllToPreset();
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
        }

        private static void CreateButtonPresetAsset(UIButton button, SerializedObject serializedObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Button Preset",
                "UIButtonPreset",
                "asset",
                "Choose a location for the new button preset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            UIButtonPreset asset = ScriptableObject.CreateInstance<UIButtonPreset>();
            AssetDatabase.CreateAsset(asset, path);
            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp != null)
            {
                presetProp.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }

            button.preset = asset;
            button.SaveAllToPreset();
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(button);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
        }

        private static void CreateTogglePresetAsset(UIToggle toggle, SerializedObject serializedObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Toggle Preset",
                "UITogglePreset",
                "asset",
                "Choose a location for the new toggle preset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            UITogglePreset asset = ScriptableObject.CreateInstance<UITogglePreset>();
            AssetDatabase.CreateAsset(asset, path);
            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            if (presetProp != null)
            {
                presetProp.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }

            toggle.preset = asset;
            toggle.SaveAllToPreset();
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(toggle);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
        }
    }
}
