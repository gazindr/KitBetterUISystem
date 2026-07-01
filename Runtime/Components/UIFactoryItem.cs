using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endif

namespace Project.UI
{
    public enum UIFactoryItemType
    {
        UIButton,
        UIToggle,
        UITab,
        UISlider,
        UIContainer,
        QueuedUIContainer,
        UIContainerWithBackground
    }

    [AddComponentMenu("UI System/Factory Item")]
    public sealed class UIFactoryItem : MonoBehaviour
    {
        [InfoBox("Editor helper. Select this object in Hierarchy to create a new UI System object.")]
        public UIFactoryItemType type;

        public Transform outputParent;

        public bool createOnSelect = true;

        public string customObjectName;

#if UNITY_EDITOR
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.85f, 0.5f)]
        private void CreateObject()
        {
            GameObject created = UISystemHierarchyFactory.CreateFromFactory(this);
            if (created != null)
            {
                Selection.activeGameObject = created;
            }
        }
#endif
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class UIFactorySelectionWatcher
    {
        private static bool isCreating;

        static UIFactorySelectionWatcher()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (isCreating)
            {
                return;
            }

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                return;
            }

            UIFactoryItem factoryItem = selected.GetComponent<UIFactoryItem>();
            if (factoryItem == null || !factoryItem.createOnSelect)
            {
                return;
            }

            isCreating = true;
            GameObject created = UISystemHierarchyFactory.CreateFromFactory(factoryItem);
            if (created != null)
            {
                Selection.activeGameObject = created;
            }

            isCreating = false;
        }
    }

    public static class UISystemHierarchyFactory
    {
        private const string RootName = "UI_System";
        private const string SelectableGroupName = "UISelectable";
        private const string ContainerGroupName = "Container";
        private const string CreateGroupName = "Create New";

        [MenuItem("GameObject/UI System/Create UI_System", false, 10)]
        [MenuItem("Tools/UI System/Create UI_System")]
        public static void CreateFullHierarchy()
        {
            GameObject root = FindOrCreateRoot();
            RectTransform selectableGroup = FindOrCreateGroup(root.transform, SelectableGroupName);
            RectTransform containerGroup = FindOrCreateGroup(root.transform, ContainerGroupName);
            RectTransform selectableCreateGroup = FindOrCreateGroup(selectableGroup, CreateGroupName);
            RectTransform containerCreateGroup = FindOrCreateGroup(containerGroup, CreateGroupName);

            EnsureFactory(selectableCreateGroup, "+ UIButton", UIFactoryItemType.UIButton, selectableGroup);
            EnsureFactory(selectableCreateGroup, "+ UIToggle", UIFactoryItemType.UIToggle, selectableGroup);
            EnsureFactory(selectableCreateGroup, "+ UITab", UIFactoryItemType.UITab, selectableGroup);
            EnsureFactory(selectableCreateGroup, "+ UISlider", UIFactoryItemType.UISlider, selectableGroup);

            EnsureFactory(containerCreateGroup, "+ UIContainer", UIFactoryItemType.UIContainer, containerGroup);
            EnsureFactory(containerCreateGroup, "+ Queued UIContainer", UIFactoryItemType.QueuedUIContainer, containerGroup);
            EnsureFactory(containerCreateGroup, "+ UIContainer With Background", UIFactoryItemType.UIContainerWithBackground, containerGroup);

            EnsureStarterObject(selectableGroup, "UIButton", UIFactoryItemType.UIButton);
            EnsureStarterObject(selectableGroup, "UIToggle", UIFactoryItemType.UIToggle);
            EnsureStarterObject(selectableGroup, "UITab", UIFactoryItemType.UITab);
            EnsureStarterObject(selectableGroup, "UISlider", UIFactoryItemType.UISlider);
            EnsureStarterObject(containerGroup, "SettingsUI", UIFactoryItemType.UIContainer);
            EnsureStarterObject(containerGroup, "QueuedPopupUI", UIFactoryItemType.QueuedUIContainer);
            EnsureStarterObject(containerGroup, "BackgroundPopupUI", UIFactoryItemType.UIContainerWithBackground);

            EnsureEventSystem();
            Selection.activeGameObject = root;
            MarkSceneDirty();
        }

        [MenuItem("GameObject/UI System/UIButton", false, 11)]
        public static void CreateButtonMenu()
        {
            SelectCreated(CreateObject(UIFactoryItemType.UIButton, GetSelectedOrDefaultParent(SelectableGroupName), null));
        }

        [MenuItem("GameObject/UI System/UIToggle", false, 12)]
        public static void CreateToggleMenu()
        {
            SelectCreated(CreateObject(UIFactoryItemType.UIToggle, GetSelectedOrDefaultParent(SelectableGroupName), null));
        }

        [MenuItem("GameObject/UI System/UITab", false, 13)]
        public static void CreateTabMenu()
        {
            SelectCreated(CreateObject(UIFactoryItemType.UITab, GetSelectedOrDefaultParent(SelectableGroupName), null));
        }

        [MenuItem("GameObject/UI System/UISlider", false, 14)]
        public static void CreateSliderMenu()
        {
            SelectCreated(CreateObject(UIFactoryItemType.UISlider, GetSelectedOrDefaultParent(SelectableGroupName), null));
        }

        [MenuItem("GameObject/UI System/UIContainer", false, 15)]
        public static void CreateContainerMenu()
        {
            SelectCreated(CreateObject(UIFactoryItemType.UIContainer, GetSelectedOrDefaultParent(ContainerGroupName), null));
        }

        public static GameObject CreateFromFactory(UIFactoryItem factoryItem)
        {
            if (factoryItem == null)
            {
                return null;
            }

            Transform parent = factoryItem.outputParent != null ? factoryItem.outputParent : factoryItem.transform.parent;
            return CreateObject(factoryItem.type, parent, factoryItem.customObjectName);
        }

        public static GameObject CreateObject(UIFactoryItemType type, Transform parent, string customName)
        {
            if (parent == null)
            {
                parent = FindOrCreateRoot().transform;
            }

            switch (type)
            {
                case UIFactoryItemType.UIToggle:
                    return CreateToggle(parent, customName);
                case UIFactoryItemType.UITab:
                    return CreateTab(parent, customName);
                case UIFactoryItemType.UISlider:
                    return CreateSlider(parent, customName);
                case UIFactoryItemType.QueuedUIContainer:
                    return CreateContainer(parent, customName, true, false);
                case UIFactoryItemType.UIContainerWithBackground:
                    return CreateContainer(parent, customName, false, true);
                case UIFactoryItemType.UIContainer:
                    return CreateContainer(parent, customName, false, false);
                default:
                    return CreateButton(parent, customName);
            }
        }

        private static GameObject FindOrCreateRoot()
        {
            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIManager));
                Undo.RegisterCreatedObjectUndo(root, "Create UI_System");
            }

            Canvas canvas = EnsureComponent<Canvas>(root);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = EnsureComponent<CanvasScaler>(root);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureComponent<GraphicRaycaster>(root);
            EnsureComponent<UIManager>(root);
            RectTransform rectTransform = root.transform as RectTransform;
            Stretch(rectTransform);
            return root;
        }

        private static RectTransform FindOrCreateGroup(Transform parent, string groupName)
        {
            Transform existing = parent.Find(groupName);
            if (existing != null)
            {
                RectTransform existingRect = existing as RectTransform;
                if (existingRect != null)
                {
                    Stretch(existingRect);
                    return existingRect;
                }
            }

            GameObject group = new GameObject(groupName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(group, "Create " + groupName);
            group.transform.SetParent(parent, false);
            RectTransform rectTransform = group.transform as RectTransform;
            Stretch(rectTransform);
            return rectTransform;
        }

        private static void EnsureFactory(Transform parent, string objectName, UIFactoryItemType type, Transform outputParent)
        {
            Transform existing = parent.Find(objectName);
            GameObject factoryObject;
            if (existing == null)
            {
                factoryObject = new GameObject(objectName, typeof(RectTransform), typeof(UIFactoryItem));
                Undo.RegisterCreatedObjectUndo(factoryObject, "Create " + objectName);
                factoryObject.transform.SetParent(parent, false);
            }
            else
            {
                factoryObject = existing.gameObject;
            }

            RectTransform rectTransform = factoryObject.transform as RectTransform;
            rectTransform.sizeDelta = new Vector2(240f, 32f);
            rectTransform.anchoredPosition = new Vector2(0f, -36f * factoryObject.transform.GetSiblingIndex());

            UIFactoryItem factoryItem = EnsureComponent<UIFactoryItem>(factoryObject);
            factoryItem.type = type;
            factoryItem.outputParent = outputParent;
            factoryItem.createOnSelect = true;
            EditorUtility.SetDirty(factoryItem);
        }

        private static void EnsureStarterObject(Transform parent, string objectName, UIFactoryItemType type)
        {
            if (parent.Find(objectName) != null)
            {
                return;
            }

            CreateObject(type, parent, objectName);
        }

        private static GameObject CreateButton(Transform parent, string customName)
        {
            GameObject buttonObject = CreateGraphicObject(UniqueName(parent, string.IsNullOrEmpty(customName) ? "UIButton" : customName), parent, new Vector2(260f, 76f), NextPosition(parent));
            Image image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.13f, 0.16f, 0.22f, 1f);

            UIButton button = EnsureComponent<UIButton>(buttonObject);
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            AddLabel(buttonObject.transform, "UIButton");
            MarkCreated(buttonObject);
            return buttonObject;
        }

        private static GameObject CreateToggle(Transform parent, string customName)
        {
            GameObject toggleObject = CreateGraphicObject(UniqueName(parent, string.IsNullOrEmpty(customName) ? "UIToggle" : customName), parent, new Vector2(220f, 78f), NextPosition(parent));
            Image baseImage = EnsureComponent<Image>(toggleObject);
            baseImage.color = new Color(0.11f, 0.13f, 0.18f, 1f);

            UIToggle toggle = EnsureComponent<UIToggle>(toggleObject);
            toggle.targetGraphic = baseImage;
            toggle.transition = Selectable.Transition.None;

            RectTransform background = CreateChildGraphic(toggleObject.transform, "Background", new Vector2(112f, 44f), new Vector2(-34f, 0f), new Color(0.25f, 0.28f, 0.35f, 1f));
            RectTransform handle = CreateChildGraphic(background, "Handle", new Vector2(36f, 36f), new Vector2(-32f, 0f), new Color(0.82f, 0.88f, 0.95f, 1f));
            toggle.backgroundTarget = background;
            toggle.handleTarget = handle;

            ConfigureToggleAnimation(toggle);
            AddLabel(toggleObject.transform, "UIToggle");
            MarkCreated(toggleObject);
            return toggleObject;
        }

        private static GameObject CreateTab(Transform parent, string customName)
        {
            GameObject tabObject = CreateGraphicObject(UniqueName(parent, string.IsNullOrEmpty(customName) ? "UITab" : customName), parent, new Vector2(240f, 68f), NextPosition(parent));
            Image image = EnsureComponent<Image>(tabObject);
            image.color = new Color(0.16f, 0.18f, 0.24f, 1f);

            UITab tab = EnsureComponent<UITab>(tabObject);
            tab.targetGraphic = image;
            tab.transition = Selectable.Transition.None;
            AddLabel(tabObject.transform, "UITab");
            MarkCreated(tabObject);
            return tabObject;
        }

        private static GameObject CreateSlider(Transform parent, string customName)
        {
            GameObject sliderObject = CreateGraphicObject(UniqueName(parent, string.IsNullOrEmpty(customName) ? "UISlider" : customName), parent, new Vector2(360f, 56f), NextPosition(parent));
            Image background = EnsureComponent<Image>(sliderObject);
            background.color = new Color(0.12f, 0.14f, 0.19f, 1f);

            RectTransform fill = CreateChildGraphic(sliderObject.transform, "Fill", new Vector2(0f, 0f), Vector2.zero, new Color(0.25f, 0.62f, 0.95f, 1f));
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0.5f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            RectTransform handle = CreateChildGraphic(sliderObject.transform, "Handle", new Vector2(38f, 38f), Vector2.zero, new Color(0.9f, 0.94f, 1f, 1f));
            handle.anchorMin = new Vector2(0.5f, 0.5f);
            handle.anchorMax = new Vector2(0.5f, 0.5f);

            UISlider slider = EnsureComponent<UISlider>(sliderObject);
            slider.targetGraphic = background;
            slider.transition = Selectable.Transition.None;
            slider.fillTarget = fill;
            slider.handleTarget = handle;
            slider.Value = 0.5f;
            MarkCreated(sliderObject);
            return sliderObject;
        }

        private static GameObject CreateContainer(Transform parent, string customName, bool queued, bool withBackground)
        {
            string fallbackName = withBackground ? "BackgroundPopupUI" : queued ? "QueuedPopupUI" : "SettingsUI";
            GameObject containerObject = CreateGraphicObject(UniqueName(parent, string.IsNullOrEmpty(customName) ? fallbackName : customName), parent, new Vector2(620f, 460f), NextPosition(parent));
            Image panel = EnsureComponent<Image>(containerObject);
            panel.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);
            EnsureComponent<CanvasGroup>(containerObject);

            UIContainer container = EnsureComponent<UIContainer>(containerObject);
            container.id = containerObject.name;
            container.category = string.Empty;
            container.useInQueue = queued;
            container.queueGroup = "Default";
            container.startupMode = UIContainerStartupMode.InstantHide;

            if (withBackground)
            {
                container.backgroundSettings.useBackground = true;
                container.backgroundSettings.autoCreate = true;
                container.backgroundSettings.backgroundColor = Color.black;
                container.backgroundSettings.backgroundAlpha = 0.65f;
                container.backgroundSettings.closeContainerOnClick = true;
            }

            AddLabel(containerObject.transform, containerObject.name);
            MarkCreated(containerObject);
            return containerObject;
        }

        private static GameObject CreateGraphicObject(string objectName, Transform parent, Vector2 size, Vector2 position)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + objectName);
            gameObject.transform.SetParent(parent, false);

            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            return gameObject;
        }

        private static RectTransform CreateChildGraphic(Transform parent, string objectName, Vector2 size, Vector2 position, Color color)
        {
            GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(child, "Create " + objectName);
            child.transform.SetParent(parent, false);

            RectTransform rectTransform = child.transform as RectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            Image image = child.GetComponent<Image>();
            image.color = color;
            return rectTransform;
        }

        private static void AddLabel(Transform parent, string label)
        {
            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Undo.RegisterCreatedObjectUndo(textObject, "Create Label");
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.transform as RectTransform;
            Stretch(rectTransform);

            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 24;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private static void ConfigureToggleAnimation(UIToggle toggle)
        {
            toggle.handleSelectAnimation.move.enabled = true;
            toggle.handleSelectAnimation.move.duration = 0.18f;
            toggle.handleSelectAnimation.move.fromMode = UIValueMode.CustomValue;
            toggle.handleSelectAnimation.move.customFromVector = new Vector3(-32f, 0f, 0f);
            toggle.handleSelectAnimation.move.toMode = UIValueMode.CustomValue;
            toggle.handleSelectAnimation.move.customToVector = new Vector3(32f, 0f, 0f);

            toggle.handleDeselectAnimation.move.enabled = true;
            toggle.handleDeselectAnimation.move.duration = 0.18f;
            toggle.handleDeselectAnimation.move.fromMode = UIValueMode.CurrentValue;
            toggle.handleDeselectAnimation.move.toMode = UIValueMode.CustomValue;
            toggle.handleDeselectAnimation.move.customToVector = new Vector3(-32f, 0f, 0f);

            toggle.backgroundSelectAnimation.scale.enabled = true;
            toggle.backgroundSelectAnimation.scale.duration = 0.18f;
            toggle.backgroundSelectAnimation.scale.fromMode = UIValueMode.CurrentValue;
            toggle.backgroundSelectAnimation.scale.toMode = UIValueMode.CustomValue;
            toggle.backgroundSelectAnimation.scale.customToVector = new Vector3(1.06f, 1.06f, 1f);

            toggle.backgroundDeselectAnimation.scale.enabled = true;
            toggle.backgroundDeselectAnimation.scale.duration = 0.18f;
            toggle.backgroundDeselectAnimation.scale.fromMode = UIValueMode.CurrentValue;
            toggle.backgroundDeselectAnimation.scale.toMode = UIValueMode.CustomValue;
            toggle.backgroundDeselectAnimation.scale.customToVector = Vector3.one;
        }

        private static Transform GetSelectedOrDefaultParent(string groupName)
        {
            if (Selection.activeTransform != null)
            {
                return Selection.activeTransform;
            }

            GameObject root = FindOrCreateRoot();
            return FindOrCreateGroup(root.transform, groupName);
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                Undo.AddComponent(eventSystem, inputSystemModuleType);
            }
            else
            {
                Undo.AddComponent<StandaloneInputModule>(eventSystem);
            }
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(gameObject);
        }

        private static string UniqueName(Transform parent, string baseName)
        {
            if (parent == null || parent.Find(baseName) == null)
            {
                return baseName;
            }

            int index = 2;
            string candidate = baseName + " " + index;
            while (parent.Find(candidate) != null)
            {
                index++;
                candidate = baseName + " " + index;
            }

            return candidate;
        }

        private static Vector2 NextPosition(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).GetComponent<UIFactoryItem>() == null && parent.GetChild(i).name != CreateGroupName)
                {
                    count++;
                }
            }

            return new Vector2(0f, -92f * count);
        }

        private static void Stretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        private static void SelectCreated(GameObject created)
        {
            if (created != null)
            {
                Selection.activeGameObject = created;
                MarkSceneDirty();
            }
        }

        private static void MarkCreated(GameObject gameObject)
        {
            EditorUtility.SetDirty(gameObject);
            MarkSceneDirty();
        }

        private static void MarkSceneDirty()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
    }
#endif
}



