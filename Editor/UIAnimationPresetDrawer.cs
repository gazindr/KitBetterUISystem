using System.Collections.Generic;
using Project.UI;
using UnityEditor;
using UnityEngine;

namespace Project.UI.Editor
{
    public static class UIAnimationPresetDrawer
    {
        public static void DrawContainerPresetBar(SerializedObject serializedObject, UIContainer container)
        {
            if (container == null)
            {
                return;
            }

            SerializedProperty presetProp = serializedObject.FindProperty("animationPreset");
            if (presetProp == null)
            {
                return;
            }

            UIContainerAnimationPreset preset = presetProp.objectReferenceValue as UIContainerAnimationPreset;
            bool dirty = IsDirty(preset, container.animations);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp, new GUIContent(dirty ? "Animation Preset *" : "Animation Preset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                preset = presetProp.objectReferenceValue as UIContainerAnimationPreset;
                if (preset != null)
                {
                    Undo.RecordObject(container, "Apply Container Animation Preset");
                    preset.ApplyTo(container.animations);
                    EditorUtility.SetDirty(container);
                    serializedObject.Update();
                }
            }

            using (new EditorGUI.DisabledScope(preset == null || !dirty))
            {
                if (GUILayout.Button("Save", GUILayout.Width(52f)))
                {
                    Undo.RecordObject(preset, "Save Container Animation Preset");
                    preset.CopyFrom(container.animations);
                    EditorUtility.SetDirty(preset);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(28f)))
            {
                CreateContainerPresetAsset(container, serializedObject);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }

        public static void DrawButtonPresetBar(SerializedObject serializedObject, UIButton button)
        {
            if (button == null)
            {
                return;
            }

            SerializedProperty presetProp = serializedObject.FindProperty("animationPreset");
            if (presetProp == null)
            {
                return;
            }

            UIButtonAnimationPreset preset = presetProp.objectReferenceValue as UIButtonAnimationPreset;
            bool dirty = IsDirty(preset, button.stateAnimations);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp, new GUIContent(dirty ? "Animation Preset *" : "Animation Preset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                preset = presetProp.objectReferenceValue as UIButtonAnimationPreset;
                if (preset != null)
                {
                    Undo.RecordObject(button, "Apply Button Animation Preset");
                    preset.ApplyTo(button.stateAnimations);
                    EditorUtility.SetDirty(button);
                    serializedObject.Update();
                }
            }

            using (new EditorGUI.DisabledScope(preset == null || !dirty))
            {
                if (GUILayout.Button("Save", GUILayout.Width(52f)))
                {
                    Undo.RecordObject(preset, "Save Button Animation Preset");
                    preset.CopyFrom(button.stateAnimations);
                    EditorUtility.SetDirty(preset);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(28f)))
            {
                CreateButtonPresetAsset(button, serializedObject);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }

        public static void DrawDirectionGrid(SerializedProperty directionProp, string label)
        {
            if (directionProp == null)
            {
                return;
            }

            EditorGUILayout.LabelField(label);
            Rect rect = GUILayoutUtility.GetRect(90f, 66f, GUILayout.ExpandWidth(false));
            float cell = 20f;
            float gap = 2f;
            float startX = rect.x;
            float startY = rect.y;

            UIAnimationDirection[,] map =
            {
                { UIAnimationDirection.TopLeft, UIAnimationDirection.Top, UIAnimationDirection.TopRight },
                { UIAnimationDirection.Left, UIAnimationDirection.Left, UIAnimationDirection.Right },
                { UIAnimationDirection.BottomLeft, UIAnimationDirection.Bottom, UIAnimationDirection.BottomRight }
            };

            // Center cell unused for "stay" — draw blank
            string[,] labels =
            {
                { "↖", "↑", "↗" },
                { "←", "·", "→" },
                { "↙", "↓", "↘" }
            };

            UIAnimationDirection current = (UIAnimationDirection)directionProp.enumValueIndex;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (row == 1 && col == 1)
                    {
                        continue;
                    }

                    Rect cellRect = new Rect(startX + col * (cell + gap), startY + row * (cell + gap), cell, cell);
                    bool selected = current == map[row, col];
                    Color prev = GUI.backgroundColor;
                    if (selected)
                    {
                        GUI.backgroundColor = new Color(0.35f, 0.75f, 0.4f, 1f);
                    }

                    if (GUI.Button(cellRect, labels[row, col]))
                    {
                        directionProp.enumValueIndex = (int)map[row, col];
                    }

                    GUI.backgroundColor = prev;
                }
            }
        }

        private static bool IsDirty(UIContainerAnimationPreset preset, UIContainerAnimationProfile current)
        {
            if (preset == null || preset.animations == null || current == null)
            {
                return false;
            }

            return JsonUtility.ToJson(preset.animations) != JsonUtility.ToJson(current);
        }

        private static bool IsDirty(UIButtonAnimationPreset preset, UISelectableAnimationProfile current)
        {
            if (preset == null || preset.stateAnimations == null || current == null)
            {
                return false;
            }

            return JsonUtility.ToJson(preset.stateAnimations) != JsonUtility.ToJson(current);
        }

        private static void CreateContainerPresetAsset(UIContainer container, SerializedObject serializedObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Container Animation Preset",
                "UIContainerAnimationPreset",
                "asset",
                "Choose a location for the new animation preset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            UIContainerAnimationPreset asset = ScriptableObject.CreateInstance<UIContainerAnimationPreset>();
            asset.CopyFrom(container.animations);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            SerializedProperty presetProp = serializedObject.FindProperty("animationPreset");
            if (presetProp != null)
            {
                presetProp.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUIUtility.PingObject(asset);
        }

        private static void CreateButtonPresetAsset(UIButton button, SerializedObject serializedObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Button Animation Preset",
                "UIButtonAnimationPreset",
                "asset",
                "Choose a location for the new animation preset.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            UIButtonAnimationPreset asset = ScriptableObject.CreateInstance<UIButtonAnimationPreset>();
            asset.CopyFrom(button.stateAnimations);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            SerializedProperty presetProp = serializedObject.FindProperty("animationPreset");
            if (presetProp != null)
            {
                presetProp.objectReferenceValue = asset;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUIUtility.PingObject(asset);
        }
    }
}
