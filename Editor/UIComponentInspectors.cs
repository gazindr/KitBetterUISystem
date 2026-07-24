using System.Collections.Generic;
using Project.UI;
using static Project.UI.Editor.UIInspectorDraw;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Project.UI.Editor
{
    [CustomEditor(typeof(UISelectable), true)]
    [CanEditMultipleObjects]
    public sealed class UISelectableInspector : UnityEditor.Editor
    {
        private static readonly Dictionary<int, int> MainTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> StateTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> StateAnimationTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> ToggleTargetTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> ToggleAnimationTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> BehaviourBlockTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> BehaviourEntryTabByTarget = new Dictionary<int, int>();

        private static readonly string[] MainTabs = { "Settings", "Animations", "Behaviours", "Presets", "Debug" };
        private static readonly string[] StateTabs = { "Normal", "Highlighted", "Pressed", "Selected", "Disabled" };
        private static readonly string[] StateProperties = { "normal", "highlighted", "pressed", "selected", "disabled" };
        private static readonly string[] ToggleTargetTabs = { "Bg Select", "Bg Deselect", "Handle Select", "Handle Deselect" };
        private static readonly string[] ToggleTargetProperties = { "backgroundSelectAnimation", "backgroundDeselectAnimation", "handleSelectAnimation", "handleDeselectAnimation" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UISelectable selectable = (UISelectable)target;
            int key = selectable.GetHashCode();
            DrawTitle(selectable.GetType().Name, selectable.CurrentState.ToString());
            int mainTab = DrawToolbar(MainTabByTarget, key, MainTabs);

            switch (mainTab)
            {
                case 0:
                    DrawSelectableSettings(selectable);
                    DrawTypeSpecificSettings(selectable);
                    break;
                case 1:
                    DrawStateAnimations(selectable);
                    DrawTypeSpecificAnimations(selectable);
                    break;
                case 2:
                    DrawBehaviours(selectable);
                    break;
                case 3:
                    DrawPresets();
                    break;
                default:
                    DrawSelectableDebug(selectable);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSelectableSettings(UISelectable selectable)
        {
            BeginSection("Common");
            DrawProperty("m_Interactable", "Interactable");
            DrawProperty("m_TargetGraphic", "Target Graphic");
            DrawProperty("animateStateAutomatically", "Animate State Automatically");
            DrawProperty("blockPointerWhenDisabled", "Block Pointer When Disabled");
            DrawProperty("invokeOnSubmit", "Invoke On Submit");
            DrawProperty("useInQueue", "Use In Queue");
            SerializedProperty useQueue = serializedObject.FindProperty("useInQueue");
            if (useQueue != null && useQueue.boolValue)
            {
                DrawProperty("queueGroup", "Queue Group");
                DrawProperty("queueReleaseDelay", "Queue Release Delay");
            }

            if (selectable is UIButton)
            {
                DrawProperty("clickCooldown", "Click Cooldown");
                DrawProperty("doubleClickInterval", "Double Click Interval");
                DrawProperty("longClickDuration", "Long Click Duration");
            }

            EndSection();
        }

        private void DrawTypeSpecificSettings(UISelectable selectable)
        {
            if (selectable is UIToggle)
            {
                BeginSection("Toggle");
                DrawProperty("isOn", "Is On");
                DrawProperty("multipleSelectCount", "Multiple Select Count");
                DrawProperty("resetMultipleCounterOnDeselect", "Reset Multiple Counter On Deselect");
                DrawProperty("backgroundTarget", "Background Target");
                DrawProperty("handleTarget", "Handle Target");
                EndSection();
            }
            else if (selectable is UITab)
            {
                BeginSection("Tab");
                DrawProperty("isSelected", "Is Selected");
                DrawProperty("group", "Group");
                DrawProperty("multipleSelectCount", "Multiple Select Count");
                DrawProperty("resetMultipleCounterOnDeselect", "Reset Multiple Counter On Deselect");
                DrawProperty("linkedContainer", "Linked Container");
                SerializedProperty linked = serializedObject.FindProperty("linkedContainer");
                if (linked != null && linked.objectReferenceValue != null)
                {
                    DrawProperty("showLinkedContainerOnSelect", "Show Linked On Select");
                    DrawProperty("hideLinkedContainerOnDeselect", "Hide Linked On Deselect");
                }

                EndSection();
            }
            else if (selectable is UISlider)
            {
                BeginSection("Slider");
                DrawProperty("minValue", "Min Value");
                DrawProperty("maxValue", "Max Value");
                DrawProperty("sliderValue", "Value");
                DrawProperty("wholeNumbers", "Whole Numbers");
                DrawProperty("direction", "Direction");
                DrawProperty("fillTarget", "Fill Target");
                DrawProperty("handleTarget", "Handle Target");
                EndSection();
            }
        }

        private void DrawStateAnimations(UISelectable selectable)
        {
            BeginSection("State Animations");
            int key = selectable.GetHashCode();
            SerializedProperty stateAnimations = serializedObject.FindProperty("stateAnimations");
            int stateTab = DrawToolbarWithAnimationIndicators(StateTabByTarget, key, StateTabs, stateAnimations, StateProperties);

            if (stateAnimations != null)
            {
                SerializedProperty state = stateAnimations.FindPropertyRelative(StateProperties[stateTab]);
                DrawAnimationState(state, StateAnimationTabByTarget, key);
            }

            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Play State"))
            {
                serializedObject.ApplyModifiedProperties();
                selectable.EditorPreviewState((UISelectableState)stateTab);
            }

            if (NeutralButton("Stop"))
            {
                selectable.EditorStopPreview();
            }

            if (NeutralButton("Complete"))
            {
                selectable.EditorCompletePreview();
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private void DrawTypeSpecificAnimations(UISelectable selectable)
        {
            UIToggle toggle = selectable as UIToggle;
            if (toggle == null)
            {
                return;
            }

            BeginSection("Toggle Target Animations");
            int key = selectable.GetHashCode();
            int targetTab = DrawToolbar(ToggleTargetTabByTarget, key, ToggleTargetTabs);
            SerializedProperty state = serializedObject.FindProperty(ToggleTargetProperties[targetTab]);
            DrawAnimationState(state, ToggleAnimationTabByTarget, key);

            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Play Select"))
            {
                serializedObject.ApplyModifiedProperties();
                toggle.EditorPreviewSelect();
            }

            if (NeutralButton("Play Deselect"))
            {
                serializedObject.ApplyModifiedProperties();
                toggle.EditorPreviewDeselect();
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private void DrawBehaviours(UISelectable selectable)
        {
            BeginSection("Add Behaviour");
            SerializedProperty blocks = serializedObject.FindProperty("behaviours");
            List<UIBehaviourTrigger> availableTriggers = BuildAvailableTriggers(blocks);
            if (availableTriggers.Count == 0)
            {
                EditorGUILayout.HelpBox("All triggers are already used. Remove an existing block to make its trigger available again.", MessageType.Info);
            }
            else
            {
                SerializedProperty triggerToAdd = serializedObject.FindProperty("behaviourToAdd");
                UIBehaviourTrigger selectedTrigger = triggerToAdd == null ? availableTriggers[0] : (UIBehaviourTrigger)triggerToAdd.enumValueIndex;
                int selectedIndex = Mathf.Max(0, availableTriggers.IndexOf(selectedTrigger));
                string[] labels = BuildTriggerLabels(availableTriggers);
                selectedIndex = EditorGUILayout.Popup("Trigger To Add", selectedIndex, labels);
                selectedTrigger = availableTriggers[selectedIndex];
                if (triggerToAdd != null)
                {
                    triggerToAdd.enumValueIndex = (int)selectedTrigger;
                }

                if (GreenButton("Add Behaviour"))
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(selectable, "Add UI Behaviour");
                    selectable.AddBehaviourBlock(selectedTrigger);
                    EditorUtility.SetDirty(selectable);
                    serializedObject.Update();
                }
            }

            EndSection();

            blocks = serializedObject.FindProperty("behaviours");
            if (blocks == null || blocks.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No behaviour blocks. Choose a trigger above and press Add Behaviour.", MessageType.Info);
                return;
            }

            NormalizeBehaviourBlocks(selectable, blocks);

            BeginSection("Behaviour Blocks");
            int key = selectable.GetHashCode();
            int blockTab = DrawToolbar(BehaviourBlockTabByTarget, key, BuildBlockLabels(blocks));
            blockTab = Mathf.Clamp(blockTab, 0, blocks.arraySize - 1);
            BehaviourBlockTabByTarget[key] = blockTab;

            SerializedProperty block = blocks.GetArrayElementAtIndex(blockTab);
            DrawBehaviourBlock(selectable, blocks, block, blockTab);
            EndSection();
        }

        private void NormalizeBehaviourBlocks(UISelectable selectable, SerializedProperty blocks)
        {
            if (blocks == null || blocks.arraySize == 0)
            {
                return;
            }

            bool changed = false;
            HashSet<int> usedTriggers = new HashSet<int>();
            for (int i = blocks.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty block = blocks.GetArrayElementAtIndex(i);
                SerializedProperty trigger = block.FindPropertyRelative("trigger");
                if (trigger == null)
                {
                    continue;
                }

                if (usedTriggers.Contains(trigger.enumValueIndex))
                {
                    blocks.DeleteArrayElementAtIndex(i);
                    changed = true;
                    continue;
                }

                usedTriggers.Add(trigger.enumValueIndex);
                SerializedProperty allowDuplicates = block.FindPropertyRelative("allowDuplicates");
                if (allowDuplicates != null && allowDuplicates.boolValue)
                {
                    allowDuplicates.boolValue = false;
                    changed = true;
                }

                SerializedProperty entries = block.FindPropertyRelative("entries");
                if (entries == null)
                {
                    continue;
                }

                while (entries.arraySize > 1)
                {
                    entries.DeleteArrayElementAtIndex(entries.arraySize - 1);
                    changed = true;
                }

                if (entries.arraySize == 0)
                {
                    entries.InsertArrayElementAtIndex(0);
                    changed = true;
                }

                SerializedProperty entry = entries.GetArrayElementAtIndex(0);
                SerializedProperty name = entry.FindPropertyRelative("name");
                if (name != null)
                {
                    string triggerName = trigger.enumDisplayNames[trigger.enumValueIndex];
                    if (name.stringValue != triggerName)
                    {
                        name.stringValue = triggerName;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(selectable);
                serializedObject.Update();
            }
        }

        private void DrawBehaviourBlock(UISelectable selectable, SerializedProperty blocks, SerializedProperty block, int blockIndex)
        {
            EditorGUILayout.BeginHorizontal();
            if (RedButton("Remove Block", GUILayout.Width(150f)))
            {
                blocks.DeleteArrayElementAtIndex(blockIndex);
                return;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            SerializedProperty trigger = block.FindPropertyRelative("trigger");
            string triggerLabel = trigger == null ? "Unknown" : trigger.enumDisplayNames[trigger.enumValueIndex];
            EditorGUILayout.LabelField("Trigger", triggerLabel);
            DrawRelative(block, "enabled", "Enabled");
            DrawRelative(block, "allowWhenDisabled", "Allow When Disabled");
            DrawRelative(block, "cooldown", "Cooldown");
            DrawOdinKeyCode(block.FindPropertyRelative("keyboardKey"), "Keyboard Key",
                "Optional. If not None, this behaviour also runs on GetKeyDown of that key in Play Mode.");

            SerializedProperty entries = block.FindPropertyRelative("entries");
            if (entries == null || entries.arraySize == 0)
            {
                EditorGUILayout.HelpBox("This block will create one entry automatically.", MessageType.Info);
                return;
            }

            DrawBehaviourEntry(entries.GetArrayElementAtIndex(0), triggerLabel);
        }

        private void DrawBehaviourEntry(SerializedProperty entry, string triggerLabel)
        {
            BeginInnerSection(triggerLabel);
            DrawRelative(entry, "enabled", "Enabled");
            DrawRelative(entry, "delay", "Delay");
            DrawRelative(entry, "executeOnce", "Execute Once");
            DrawRelative(entry, "debugLogging", "Log Execution", false, "Writes one Console log when this behaviour executes.");
            DrawRelative(entry, "targetContainer", "Target Container Override", false, "Optional. Show/Hide/Toggle Container actions use this container when their own target is empty.");
            DrawActionReferences(entry.FindPropertyRelative("actions"));
            DrawRelative(entry, "callback", "Unity Event", true);
            EndSection();
        }

        private void DrawActionReferences(SerializedProperty actions)
        {
            if (actions == null)
            {
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            for (int i = 0; i < actions.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(actions.GetArrayElementAtIndex(i), GUIContent.none);
                if (RedButton("-", GUILayout.Width(34f)))
                {
                    actions.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GreenButton("Add Action Slot"))
            {
                actions.InsertArrayElementAtIndex(actions.arraySize);
                SerializedProperty added = actions.GetArrayElementAtIndex(actions.arraySize - 1);
                if (added != null)
                {
                    added.objectReferenceValue = null;
                }
            }
        }

        private void DrawPresets()
        {
            BeginSection("Presets");
            DrawProperty("preset", "Preset");
            DrawProperty("presetApplyMask", "Apply Mask", true);
            EndSection();
        }

        private void DrawSelectableDebug(UISelectable selectable)
        {
            BeginSection("Debug");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Current State", selectable.CurrentState);
            }

            DrawProperty("previewState", "Preview State");
            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Play Preview State"))
            {
                serializedObject.ApplyModifiedProperties();
                selectable.EditorPreviewState(selectable.previewState);
            }

            if (NeutralButton("Execute Trigger"))
            {
                serializedObject.ApplyModifiedProperties();
                selectable.ExecuteTrigger(selectable.behaviourToAdd);
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private static string[] BuildBlockLabels(SerializedProperty blocks)
        {
            string[] labels = new string[blocks.arraySize];
            for (int i = 0; i < blocks.arraySize; i++)
            {
                SerializedProperty block = blocks.GetArrayElementAtIndex(i);
                SerializedProperty trigger = block.FindPropertyRelative("trigger");
                string label = trigger == null ? "Block " + (i + 1) : trigger.enumDisplayNames[trigger.enumValueIndex];
                SerializedProperty key = block.FindPropertyRelative("keyboardKey");
                if (key != null && key.enumValueIndex > 0)
                {
                    label += " [" + key.enumDisplayNames[key.enumValueIndex] + "]";
                }

                labels[i] = label;
            }

            return labels;
        }

        private static List<UIBehaviourTrigger> BuildAvailableTriggers(SerializedProperty blocks)
        {
            List<UIBehaviourTrigger> triggers = new List<UIBehaviourTrigger>();
            UIBehaviourTrigger[] all = (UIBehaviourTrigger[])System.Enum.GetValues(typeof(UIBehaviourTrigger));
            for (int i = 0; i < all.Length; i++)
            {
                if (!IsTriggerUsed(blocks, all[i]))
                {
                    triggers.Add(all[i]);
                }
            }

            return triggers;
        }

        private static bool IsTriggerUsed(SerializedProperty blocks, UIBehaviourTrigger trigger)
        {
            if (blocks == null)
            {
                return false;
            }

            for (int i = 0; i < blocks.arraySize; i++)
            {
                SerializedProperty block = blocks.GetArrayElementAtIndex(i);
                SerializedProperty blockTrigger = block.FindPropertyRelative("trigger");
                if (blockTrigger != null && blockTrigger.enumValueIndex == (int)trigger)
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] BuildTriggerLabels(List<UIBehaviourTrigger> triggers)
        {
            string[] labels = new string[triggers.Count];
            for (int i = 0; i < triggers.Count; i++)
            {
                labels[i] = ObjectNames.NicifyVariableName(triggers[i].ToString());
            }

            return labels;
        }

        private void DrawProperty(string propertyName, string label, bool includeChildren = false)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
            }
        }

        private static void DrawRelative(SerializedProperty root, string propertyName, string label, bool includeChildren = false, string tooltip = null)
        {
            SerializedProperty property = root.FindPropertyRelative(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), includeChildren);
            }
        }

        /// <summary>
        /// Odin searchable enum dropdown (same UX as ShopButton KeyCode / Input System Key selectors).
        /// </summary>
        private static void DrawOdinKeyCode(SerializedProperty property, string label, string tooltip = null)
        {
            if (property == null)
            {
                return;
            }

            KeyCode current = (KeyCode)property.intValue;
            EditorGUI.BeginChangeCheck();
            KeyCode next = EnumSelector<KeyCode>.DrawEnumField(new GUIContent(label, tooltip), current);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = (int)next;
            }
        }
    }

    [CustomEditor(typeof(UIContainer))]
    [CanEditMultipleObjects]
    public sealed class UIContainerInspector : UnityEditor.Editor
    {
        private static readonly Dictionary<int, int> MainTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> ContainerStateTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> ContainerAnimationTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> BackgroundStateTabByTarget = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> BackgroundAnimationTabByTarget = new Dictionary<int, int>();

        private static readonly string[] MainTabs = { "Settings", "Animations", "Background", "Callbacks", "Presets", "Debug" };
        private static readonly string[] ContainerStateTabs = { "Show", "Hide" };
        private static readonly string[] ContainerStateProperties = { "show", "hide" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UIContainer container = (UIContainer)target;
            int key = container.GetHashCode();
            DrawTitle("UIContainer", container.State.ToString());
            int mainTab = DrawToolbar(MainTabByTarget, key, MainTabs);

            switch (mainTab)
            {
                case 0:
                    DrawSettings();
                    break;
                case 1:
                    DrawAnimations(container);
                    break;
                case 2:
                    DrawBackground();
                    break;
                case 3:
                    DrawCallbacks();
                    break;
                case 4:
                    DrawPresets();
                    break;
                default:
                    DrawDebug(container);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettings()
        {
            BeginSection("Settings");
            DrawProperty("id", "Id");
            DrawProperty("category", "Category");
            DrawProperty("autoRegister", "Auto Register");
            DrawProperty("registerOnAwake", "Register On Awake");
            DrawProperty("startupMode", "Startup Mode");
            DrawQueueSettings();
            DrawProperty("useAutoHide", "Use Auto Hide");
            SerializedProperty useAutoHide = serializedObject.FindProperty("useAutoHide");
            if (useAutoHide != null && useAutoHide.boolValue)
            {
                DrawProperty("autoHideDelay", "Auto Hide Delay");
            }

            EndSection();
        }

        private void DrawQueueSettings()
        {
            SerializedProperty useQueue = serializedObject.FindProperty("useInQueue");
            SerializedProperty showDelay = serializedObject.FindProperty("queueShowDelay");
            EditorGUILayout.BeginHorizontal();
            if (useQueue != null)
            {
                EditorGUILayout.PropertyField(useQueue, new GUIContent("Use In Queue"));
            }

            if (useQueue != null && useQueue.boolValue && showDelay != null)
            {
                EditorGUILayout.PropertyField(showDelay, new GUIContent("Show Delay"));
            }

            EditorGUILayout.EndHorizontal();

            if (useQueue != null && useQueue.boolValue)
            {
                DrawProperty("queueGroup", "Queue Group");
            }
        }

        private void DrawAnimations(UIContainer container)
        {
            BeginSection("Container Animations");
            int key = container.GetHashCode();
            SerializedProperty animations = serializedObject.FindProperty("animations");
            int stateTab = DrawToolbarWithAnimationIndicators(ContainerStateTabByTarget, key, ContainerStateTabs, animations, ContainerStateProperties);

            if (animations != null)
            {
                SerializedProperty state = animations.FindPropertyRelative(ContainerStateProperties[stateTab]);
                DrawAnimationState(state, ContainerAnimationTabByTarget, key);
            }

            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Play Show"))
            {
                serializedObject.ApplyModifiedProperties();
                container.EditorPreviewShowAnimation();
            }

            if (NeutralButton("Play Hide"))
            {
                serializedObject.ApplyModifiedProperties();
                container.EditorPreviewHideAnimation();
            }

            if (NeutralButton("Stop"))
            {
                container.EditorStopPreview();
            }

            if (NeutralButton("Complete"))
            {
                container.EditorCompletePreview();
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private void DrawBackground()
        {
            BeginSection("Background");
            SerializedProperty background = serializedObject.FindProperty("backgroundSettings");
            if (background == null)
            {
                EndSection();
                return;
            }

            SerializedProperty useBackground = background.FindPropertyRelative("useBackground");
            EditorGUILayout.PropertyField(useBackground, new GUIContent("Use Background"));
            if (useBackground == null || !useBackground.boolValue)
            {
                EndSection();
                return;
            }

            DrawRelative(background, "backgroundInstance", "Background Instance");
            DrawRelative(background, "backgroundPrefab", "Background Prefab");
            DrawRelative(background, "autoCreate", "Auto Create");
            DrawRelative(background, "backgroundColor", "Color");
            DrawRelative(background, "backgroundAlpha", "Alpha");
            DrawRelative(background, "raycastTarget", "Raycast Target");
            DrawRelative(background, "closeContainerOnClick", "Close On Click");
            DrawRelative(background, "waitForBackgroundBeforeContainer", "Wait Before Container");
            DrawRelative(background, "backgroundToContainerDelay", "Background To Container Delay");
            DrawRelative(background, "waitForContainerBeforeBackground", "Wait Before Background");
            DrawRelative(background, "containerToBackgroundDelay", "Container To Background Delay");

            SerializedProperty animations = background.FindPropertyRelative("animations");
            if (animations != null)
            {
                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Background Animations", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);
                int key = target.GetHashCode();
                int stateTab = DrawToolbarWithAnimationIndicators(BackgroundStateTabByTarget, key, ContainerStateTabs, animations, ContainerStateProperties);
                SerializedProperty state = animations.FindPropertyRelative(ContainerStateProperties[stateTab]);
                DrawAnimationState(state, BackgroundAnimationTabByTarget, key);
            }

            EndSection();
        }

        private void DrawCallbacks()
        {
            BeginSection("Callbacks");
            DrawProperty("onShow", "On Show", true);
            DrawProperty("onVisible", "On Visible", true);
            DrawProperty("onHide", "On Hide", true);
            DrawProperty("onHidden", "On Hidden", true);
            DrawProperty("visibilityChanged", "Visibility Changed", true);
            EndSection();
        }

        private void DrawPresets()
        {
            BeginSection("Presets");
            DrawProperty("preset", "Preset");
            DrawProperty("presetApplyMask", "Apply Mask", true);
            EndSection();
        }

        private void DrawDebug(UIContainer container)
        {
            BeginSection("Debug");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("State", container.State);
            }

            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Runtime Show"))
            {
                container.Show();
            }

            if (NeutralButton("Show Isolated"))
            {
                container.ShowIsolated();
            }

            if (NeutralButton("Runtime Hide"))
            {
                container.Hide();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (NeutralButton("Instant Show"))
            {
                container.InstantShow();
            }

            if (NeutralButton("Instant Hide"))
            {
                container.InstantHide();
            }

            EditorGUILayout.EndHorizontal();
            EndSection();
        }

        private void DrawProperty(string propertyName, string label, bool includeChildren = false)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
            }
        }
    }

    public static class UIInspectorDraw
    {
        private const float ToolbarHeight = 30f;
        private const float AnimationToolbarHeight = 34f;
        private const float IndicatorBarHeight = 3f;
        private const float IndicatorGap = 2f;
        private const float IndicatorTopPadding = 6f;
        private const float AnimationIconSize = 16f;
        private const float AnimationIconSpacing = 6f;
        private const float ButtonHeight = 27f;
        private const float TwoColumnMinWidth = 760f;
        private const float FieldColumnLabelWidth = 86f;

        private static readonly string[] AnimationTabs = { "Move", "Rotate", "Scale", "Fade" };
        private static readonly string[] AnimationProperties = { "move", "rotate", "scale", "fade" };
        private static readonly string[] AnimationIconFiles =
        {
            "Move.png",
            "Rotate.png",
            "Scale.png",
            "Fade.png"
        };

        private static readonly Color MoveColor = new Color(0.55f, 0.92f, 0.28f, 1f);
        private static readonly Color RotateColor = new Color(1f, 0.55f, 0.12f, 1f);
        private static readonly Color ScaleColor = new Color(0.9f, 0.2f, 0.45f, 1f);
        private static readonly Color FadeColor = new Color(0.62f, 0.28f, 0.88f, 1f);
        private static readonly Color[] AnimationColors = { MoveColor, RotateColor, ScaleColor, FadeColor };

        private static Texture2D[] animationIcons;
        private static GUIStyle toolbarStyle;
        private static GUIStyle animationToolbarStyle;
        private static GUIStyle animationToolbarSelectedStyle;
        private static GUIStyle animationToolbarLabelStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle sectionStyle;
        private static GUIStyle innerSectionStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle subHeaderStyle;
        private static GUIStyle buttonStyle;

        public static int DrawToolbar(Dictionary<int, int> tabs, int key, string[] labels)
        {
            EditorGUILayout.Space(4f);
            int value = GetTab(tabs, key, labels.Length);
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.45f, 0.58f, 0.72f, 1f);
            value = GUILayout.Toolbar(value, labels, ToolbarStyle, GUILayout.Height(ToolbarHeight));
            GUI.backgroundColor = previous;
            tabs[key] = value;
            EditorGUILayout.Space(6f);
            return value;
        }

        public static int DrawToolbarWithAnimationIndicators(
            Dictionary<int, int> tabs,
            int key,
            string[] labels,
            SerializedProperty statesRoot,
            string[] stateProperties)
        {
            EditorGUILayout.Space(6f);
            int value = GetTab(tabs, key, labels.Length);
            float totalHeight = IndicatorTopPadding + IndicatorBarHeight + 4f + ToolbarHeight;
            Rect toolbarRect = GUILayoutUtility.GetRect(0f, totalHeight, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
            float tabWidth = toolbarRect.width / labels.Length;
            float buttonY = toolbarRect.y + IndicatorTopPadding + IndicatorBarHeight + 4f;

            for (int i = 0; i < labels.Length; i++)
            {
                Rect tabRect = new Rect(toolbarRect.x + tabWidth * i + 1f, buttonY, tabWidth - 2f, ToolbarHeight);
                bool selected = i == value;
                Color previous = GUI.backgroundColor;
                GUI.backgroundColor = selected ? new Color(0.34f, 0.5f, 0.72f, 1f) : new Color(0.36f, 0.43f, 0.5f, 1f);
                if (GUI.Button(tabRect, labels[i], ToolbarStyle))
                {
                    value = i;
                }

                GUI.backgroundColor = previous;

                if (statesRoot != null && stateProperties != null && i < stateProperties.Length)
                {
                    SerializedProperty state = statesRoot.FindPropertyRelative(stateProperties[i]);
                    bool[] enabled = GetEnabledFlags(state);
                    Rect indicatorRow = new Rect(tabRect.x + 4f, toolbarRect.y + IndicatorTopPadding, tabRect.width - 8f, IndicatorBarHeight);
                    DrawStateIndicatorBars(indicatorRow, enabled);
                }
            }

            tabs[key] = value;
            EditorGUILayout.Space(8f);
            return value;
        }

        public static void DrawAnimationState(SerializedProperty state, Dictionary<int, int> animationTabs, int key)
        {
            if (state == null)
            {
                EditorGUILayout.HelpBox("Animation state is missing.", MessageType.Warning);
                return;
            }

            int animationTab = DrawAnimationToolbar(animationTabs, key, state);
            SerializedProperty settings = state.FindPropertyRelative(AnimationProperties[animationTab]);
            DrawAnimationSettings(settings, (UIAnimationType)animationTab);
        }

        private static int DrawAnimationToolbar(Dictionary<int, int> tabs, int key, SerializedProperty state)
        {
            EditorGUILayout.Space(4f);
            int value = GetTab(tabs, key, AnimationTabs.Length);
            Texture2D[] icons = GetAnimationIcons();
            bool[] enabled = GetEnabledFlags(state);
            float totalHeight = IndicatorTopPadding + IndicatorBarHeight + 3f + AnimationToolbarHeight;
            Rect toolbarRect = GUILayoutUtility.GetRect(0f, totalHeight, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
            float tabWidth = toolbarRect.width / AnimationTabs.Length;
            float buttonY = toolbarRect.y + IndicatorTopPadding + IndicatorBarHeight + 3f;

            for (int i = 0; i < AnimationTabs.Length; i++)
            {
                Rect tabRect = new Rect(toolbarRect.x + tabWidth * i + 1f, buttonY, tabWidth - 2f, AnimationToolbarHeight);
                bool selected = i == value;
                bool isEnabled = enabled != null && i < enabled.Length && enabled[i];
                Color previous = GUI.backgroundColor;
                if (selected && isEnabled)
                {
                    Color tint = AnimationColors[i];
                    GUI.backgroundColor = Color.Lerp(new Color(0.28f, 0.32f, 0.38f, 1f), tint, 0.35f);
                }
                else if (selected)
                {
                    GUI.backgroundColor = new Color(0.34f, 0.5f, 0.72f, 1f);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.36f, 0.43f, 0.5f, 1f);
                }

                if (GUI.Button(tabRect, GUIContent.none, selected ? AnimationToolbarSelectedStyle : AnimationToolbarStyle))
                {
                    value = i;
                }

                GUI.backgroundColor = previous;

                if (isEnabled)
                {
                    Rect barRect = new Rect(tabRect.x + 3f, toolbarRect.y + IndicatorTopPadding, tabRect.width - 6f, IndicatorBarHeight);
                    EditorGUI.DrawRect(barRect, AnimationColors[i]);
                }

                DrawAnimationTabContent(tabRect, AnimationTabs[i], icons[i], isEnabled ? AnimationColors[i] : (Color?)null, selected);
            }

            tabs[key] = value;
            EditorGUILayout.Space(8f);
            return value;
        }

        private static void DrawAnimationTabContent(Rect tabRect, string label, Texture2D icon, Color? accent, bool selected)
        {
            GUIStyle labelStyle = AnimationToolbarLabelStyle;
            Color previous = labelStyle.normal.textColor;
            if (accent.HasValue && selected)
            {
                labelStyle.normal.textColor = Color.Lerp(previous, accent.Value, 0.75f);
            }

            GUIContent labelContent = new GUIContent(label);
            Vector2 labelSize = labelStyle.CalcSize(labelContent);
            float iconWidth = icon == null ? 0f : AnimationIconSize;
            float spacing = icon == null ? 0f : AnimationIconSpacing;
            float contentWidth = iconWidth + spacing + labelSize.x;
            float startX = tabRect.x + (tabRect.width - contentWidth) * 0.5f;
            float centerY = tabRect.y + tabRect.height * 0.5f;

            if (icon != null)
            {
                Rect iconRect = new Rect(startX, centerY - AnimationIconSize * 0.5f, AnimationIconSize, AnimationIconSize);
                if (accent.HasValue)
                {
                    Color previousContent = GUI.color;
                    GUI.color = Color.Lerp(Color.white, accent.Value, selected ? 0.55f : 0.35f);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                    GUI.color = previousContent;
                }
                else
                {
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                }
            }

            Rect labelRect = new Rect(startX + iconWidth + spacing, tabRect.y, labelSize.x, tabRect.height);
            GUI.Label(labelRect, labelContent, labelStyle);
            labelStyle.normal.textColor = previous;
        }

        private static bool[] GetEnabledFlags(SerializedProperty state)
        {
            bool[] enabled = new bool[AnimationProperties.Length];
            if (state == null)
            {
                return enabled;
            }

            for (int i = 0; i < AnimationProperties.Length; i++)
            {
                SerializedProperty settings = state.FindPropertyRelative(AnimationProperties[i]);
                SerializedProperty enabledProp = settings == null ? null : settings.FindPropertyRelative("enabled");
                enabled[i] = enabledProp != null && enabledProp.boolValue;
            }

            return enabled;
        }

        private static void DrawStateIndicatorBars(Rect row, bool[] enabled)
        {
            if (enabled == null)
            {
                return;
            }

            int count = 0;
            for (int i = 0; i < enabled.Length; i++)
            {
                if (enabled[i])
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return;
            }

            float totalGaps = IndicatorGap * (count - 1);
            float barWidth = (row.width - totalGaps) / count;
            float x = row.x;
            for (int i = 0; i < enabled.Length; i++)
            {
                if (!enabled[i])
                {
                    continue;
                }

                EditorGUI.DrawRect(new Rect(x, row.y, barWidth, row.height), AnimationColors[i]);
                x += barWidth + IndicatorGap;
            }
        }

        private static Texture2D[] GetAnimationIcons()
        {
            if (animationIcons != null)
            {
                return animationIcons;
            }

            animationIcons = new Texture2D[AnimationIconFiles.Length];
            for (int i = 0; i < AnimationIconFiles.Length; i++)
            {
                animationIcons[i] = LoadAnimationIcon(AnimationIconFiles[i]);
            }

            return animationIcons;
        }

        private static Texture2D LoadAnimationIcon(string fileName)
        {
            string assetPath = "Assets/UISystem/" + fileName;
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (icon != null)
            {
                return icon;
            }

            string packagePath = "Packages/com.yeen.ui-system/" + fileName;
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(packagePath);
            if (icon != null)
            {
                return icon;
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string[] guids = AssetDatabase.FindAssets(name + " t:Texture2D");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith("/" + fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }

            return null;
        }

        public static void DrawAnimationSettings(SerializedProperty settings, UIAnimationType type)
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Animation settings are missing.", MessageType.Warning);
                return;
            }

            BeginInnerSection(type + " Animation");
            SerializedProperty enabled = settings.FindPropertyRelative("enabled");
            Color previousBg = GUI.backgroundColor;
            if (enabled != null && enabled.boolValue)
            {
                GUI.backgroundColor = Color.Lerp(GUI.backgroundColor, AnimationColors[(int)type], 0.45f);
            }

            DrawRelative(settings, "enabled", type + " Enabled");
            GUI.backgroundColor = previousBg;
            if (enabled == null || !enabled.boolValue)
            {
                EditorGUILayout.HelpBox(type + " animation is disabled.", MessageType.None);
                EndSection();
                return;
            }

            BeginInnerSection("Timing");
            DrawTwoColumnProperties(settings, "delay", "Delay", "duration", "Duration");
            DrawRelative(settings, "useUnscaledTime", "Use Unscaled Time");
            EndSection();

            BeginInnerSection("Ease");
            DrawRelative(settings, "easeMode", "Ease Mode");

            SerializedProperty easeMode = settings.FindPropertyRelative("easeMode");
            if (easeMode != null && easeMode.enumValueIndex == (int)UIEaseMode.CustomCurve)
            {
                DrawRelative(settings, "customEase", "Custom Ease");
            }
            EndSection();

            BeginInnerSection("Values");
            DrawValueColumns(settings, type);
            EndSection();

            BeginInnerSection("Loop");
            DrawRelative(settings, "playMode", "Play Mode");
            SerializedProperty playMode = settings.FindPropertyRelative("playMode");
            if (playMode != null && playMode.enumValueIndex != (int)UIAnimationPlayMode.Once)
            {
                DrawRelative(settings, "loopCount", "Loop Count");
                DrawRelative(settings, "loopDelay", "Loop Delay");
            }
            EndSection();
            EndSection();
        }

        private static void DrawValueFields(SerializedProperty settings, string prefix, UIAnimationType type)
        {
            SerializedProperty mode = settings.FindPropertyRelative(prefix + "Mode");
            if (mode == null)
            {
                return;
            }

            UIValueMode valueMode = (UIValueMode)mode.enumValueIndex;
            bool fade = type == UIAnimationType.Fade;
            if (valueMode == UIValueMode.CustomValue)
            {
                DrawRelative(settings, fade ? "custom" + Upper(prefix) + "Float" : "custom" + Upper(prefix) + "Vector", "Custom " + Upper(prefix));
            }
            else if (valueMode == UIValueMode.OffsetFromStart || valueMode == UIValueMode.OffsetFromCurrent)
            {
                DrawRelative(settings, fade ? prefix + "FloatOffset" : prefix + "VectorOffset", "Offset");
            }
        }

        private static void DrawTwoColumnProperties(SerializedProperty root, string firstProperty, string firstLabel, string secondProperty, string secondLabel)
        {
            SerializedProperty first = root.FindPropertyRelative(firstProperty);
            SerializedProperty second = root.FindPropertyRelative(secondProperty);
            if (EditorGUIUtility.currentViewWidth < TwoColumnMinWidth)
            {
                DrawProperty(first, firstLabel);
                DrawProperty(second, secondLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawPropertyColumn(first, firstLabel);
            GUILayout.Space(8f);
            DrawPropertyColumn(second, secondLabel);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawValueColumns(SerializedProperty settings, UIAnimationType type)
        {
            bool useColumns = EditorGUIUtility.currentViewWidth >= TwoColumnMinWidth;
            if (useColumns)
            {
                EditorGUILayout.BeginHorizontal();
            }

            BeginValueColumn("From");
            DrawRelative(settings, "fromMode", "Mode");
            DrawValueFields(settings, "from", type);
            EndValueColumn();

            if (useColumns)
            {
                GUILayout.Space(8f);
            }

            BeginValueColumn("To");
            DrawRelative(settings, "toMode", "Mode");
            DrawValueFields(settings, "to", type);
            EndValueColumn();

            if (useColumns)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void BeginValueColumn(string title)
        {
            EditorGUILayout.BeginVertical(InnerSectionStyle, GUILayout.MinWidth(0f));
            EditorGUILayout.LabelField(title, SubHeaderStyle);
        }

        private static void EndValueColumn()
        {
            EditorGUILayout.EndVertical();
        }

        private static void DrawPropertyColumn(SerializedProperty property, string label)
        {
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(0f));
            DrawProperty(property, label);
            EditorGUILayout.EndVertical();
        }

        public static void DrawRelative(SerializedProperty root, string propertyName, string label, bool includeChildren = false)
        {
            SerializedProperty property = root.FindPropertyRelative(propertyName);
            DrawProperty(property, label, includeChildren);
        }

        private static void DrawProperty(SerializedProperty property, string label, bool includeChildren = false)
        {
            if (property != null)
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = previousLabelWidth <= 0f ? FieldColumnLabelWidth : Mathf.Min(previousLabelWidth, FieldColumnLabelWidth);
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
                EditorGUIUtility.labelWidth = previousLabelWidth;
            }
        }

        public static void DrawTitle(string title, string state)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginVertical(SectionStyle);
            EditorGUILayout.LabelField(title, state, TitleStyle, GUILayout.Height(24f));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }

        public static bool GreenButton(string label, params GUILayoutOption[] options)
        {
            return ColoredButton(label, new Color(0.34f, 0.72f, 0.42f, 1f), options);
        }

        public static bool RedButton(string label, params GUILayoutOption[] options)
        {
            return ColoredButton(label, new Color(0.78f, 0.32f, 0.32f, 1f), options);
        }

        public static bool NeutralButton(string label, params GUILayoutOption[] options)
        {
            return ColoredButton(label, new Color(0.58f, 0.58f, 0.58f, 1f), options);
        }

        public static bool ColoredButton(string label, Color color, params GUILayoutOption[] options)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool clicked = GUILayout.Button(label, ButtonStyle, WithDefaultHeight(options));
            GUI.backgroundColor = previous;
            return clicked;
        }

        public static void BeginSection(string title)
        {
            EditorGUILayout.Space(8f);
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.74f, 0.74f, 0.74f, 1f);
            EditorGUILayout.BeginVertical(SectionStyle);
            GUI.backgroundColor = previous;
            EditorGUILayout.LabelField(title, HeaderStyle);
            EditorGUILayout.Space(4f);
        }

        public static void BeginInnerSection(string title)
        {
            EditorGUILayout.Space(6f);
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            EditorGUILayout.BeginVertical(InnerSectionStyle);
            GUI.backgroundColor = previous;
            EditorGUILayout.LabelField(title, SubHeaderStyle);
            EditorGUILayout.Space(2f);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        public static int GetTab(Dictionary<int, int> tabs, int key, int max)
        {
            if (max <= 0)
            {
                return 0;
            }

            int value;
            return tabs.TryGetValue(key, out value) ? Mathf.Clamp(value, 0, max - 1) : 0;
        }

        private static string Upper(string value)
        {
            return string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static GUILayoutOption[] WithDefaultHeight(GUILayoutOption[] options)
        {
            List<GUILayoutOption> result = options == null ? new List<GUILayoutOption>() : new List<GUILayoutOption>(options);
            result.Add(GUILayout.Height(ButtonHeight));
            return result.ToArray();
        }

        private static GUIStyle ToolbarStyle
        {
            get
            {
                if (toolbarStyle == null)
                {
                    toolbarStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        fixedHeight = ToolbarHeight,
                        margin = new RectOffset(0, 0, 2, 2),
                        padding = new RectOffset(6, 6, 3, 3)
                    };
                }

                return toolbarStyle;
            }
        }

        private static GUIStyle AnimationToolbarStyle
        {
            get
            {
                if (animationToolbarStyle == null)
                {
                    animationToolbarStyle = new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = AnimationToolbarHeight,
                        margin = new RectOffset(0, 0, 2, 2),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }

                return animationToolbarStyle;
            }
        }

        private static GUIStyle AnimationToolbarSelectedStyle
        {
            get
            {
                if (animationToolbarSelectedStyle == null)
                {
                    animationToolbarSelectedStyle = new GUIStyle(AnimationToolbarStyle);
                }

                return animationToolbarSelectedStyle;
            }
        }

        private static GUIStyle AnimationToolbarLabelStyle
        {
            get
            {
                if (animationToolbarLabelStyle == null)
                {
                    animationToolbarLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 13,
                        normal = { textColor = EditorStyles.label.normal.textColor }
                    };
                }

                return animationToolbarLabelStyle;
            }
        }

        private static GUIStyle ButtonStyle
        {
            get
            {
                if (buttonStyle == null)
                {
                    buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13,
                        fixedHeight = ButtonHeight,
                        padding = new RectOffset(8, 8, 3, 3)
                    };
                }

                return buttonStyle;
            }
        }

        private static GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 13
                    };
                }

                return titleStyle;
            }
        }

        private static GUIStyle HeaderStyle
        {
            get
            {
                if (headerStyle == null)
                {
                    headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 13,
                        margin = new RectOffset(0, 0, 2, 5)
                    };
                }

                return headerStyle;
            }
        }

        private static GUIStyle SubHeaderStyle
        {
            get
            {
                if (subHeaderStyle == null)
                {
                    subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 12,
                        margin = new RectOffset(0, 0, 1, 4)
                    };
                }

                return subHeaderStyle;
            }
        }

        private static GUIStyle SectionStyle
        {
            get
            {
                if (sectionStyle == null)
                {
                    sectionStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(12, 12, 10, 12),
                        margin = new RectOffset(0, 0, 6, 8)
                    };
                }

                return sectionStyle;
            }
        }

        private static GUIStyle InnerSectionStyle
        {
            get
            {
                if (innerSectionStyle == null)
                {
                    innerSectionStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 8, 10),
                        margin = new RectOffset(0, 0, 5, 5)
                    };
                }

                return innerSectionStyle;
            }
        }
    }
}
