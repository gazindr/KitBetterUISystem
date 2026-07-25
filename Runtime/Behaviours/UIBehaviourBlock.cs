using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [Serializable]
    public sealed class UIBehaviourBlock
    {
        [HorizontalGroup("Header")]
        [ValueDropdown(nameof(GetTriggerOptions))]
        public UIBehaviourTrigger trigger = UIBehaviourTrigger.PointerLeftClick;

        [HorizontalGroup("Header", Width = 70)]
        [LabelWidth(50)]
        public bool enabled = true;

        public bool allowWhenDisabled;

        [HideInInspector]
        public bool allowDuplicates;

        [MinValue(0f)]
        public float cooldown;

        [Tooltip("Если не None — этот behaviour также срабатывает по GetKeyDown этой клавиши (в Play Mode).")]
        [LabelText("Keyboard Key")]
        public KeyCode keyboardKey = KeyCode.None;

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
        public List<UIBehaviourEntry> entries = new List<UIBehaviourEntry>();

        [NonSerialized]
        private float lastExecuteTime = -999999f;

        public bool CanExecute(bool sourceInteractable)
        {
            if (!enabled)
            {
                return false;
            }

            if (!sourceInteractable && !allowWhenDisabled)
            {
                return false;
            }

            return cooldown <= 0f || Time.unscaledTime - lastExecuteTime >= cooldown;
        }

        public void Execute(UIBehaviourContext context, MonoBehaviour coroutineHost, bool sourceInteractable)
        {
            if (context == null || !CanExecute(sourceInteractable))
            {
                return;
            }

            lastExecuteTime = Time.unscaledTime;
            EnsureSingleEntry();

            UIBehaviourEntry entry = entries.Count == 0 ? null : entries[0];
            if (entry != null)
            {
                entry.Execute(context, coroutineHost);
            }
        }

        public void ResetRuntimeState()
        {
            lastExecuteTime = -999999f;
            EnsureSingleEntry();
            if (entries.Count == 0 || entries[0] == null)
            {
                return;
            }

            entries[0].ResetRuntimeState();
        }

        public float GetEstimatedDuration()
        {
            if (!enabled)
            {
                return 0f;
            }

            EnsureSingleEntry();
            if (entries.Count == 0 || entries[0] == null)
            {
                return 0f;
            }

            return entries[0].GetEstimatedDuration();
        }

        public UIBehaviourBlock Clone(bool includeCallbacks)
        {
            UIBehaviourBlock clone = new UIBehaviourBlock();
            clone.CopyFrom(this, includeCallbacks);
            return clone;
        }

        public void CopyFrom(UIBehaviourBlock source, bool includeCallbacks)
        {
            if (source == null)
            {
                return;
            }

            trigger = source.trigger;
            enabled = source.enabled;
            allowWhenDisabled = source.allowWhenDisabled;
            allowDuplicates = false;
            cooldown = source.cooldown;
            keyboardKey = source.keyboardKey;
            lastExecuteTime = -999999f;

            entries = new List<UIBehaviourEntry>();
            if (source.entries != null)
            {
                for (int i = 0; i < source.entries.Count; i++)
                {
                    if (source.entries[i] != null)
                    {
                        entries.Add(source.entries[i].Clone(includeCallbacks));
                        break;
                    }
                }
            }

            EnsureSingleEntry();
        }

        public void EnsureSingleEntry()
        {
            allowDuplicates = false;

            if (entries == null)
            {
                entries = new List<UIBehaviourEntry>();
            }

            while (entries.Count > 1)
            {
                entries.RemoveAt(entries.Count - 1);
            }

            if (entries.Count == 0 || entries[0] == null)
            {
                entries.Clear();
                entries.Add(new UIBehaviourEntry());
            }

            entries[0].name = GetDisplayName();
        }

        public UIBehaviourEntry GetSingleEntry()
        {
            EnsureSingleEntry();
            return entries[0];
        }

        public string GetDisplayName()
        {
            string name = NicifyVariableName(trigger.ToString());
            if (keyboardKey != KeyCode.None)
            {
                name += " [" + keyboardKey + "]";
            }

            return name;
        }

        private static string NicifyVariableName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            List<char> result = new List<char>(value.Length + 8);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (i > 0 && char.IsUpper(character) && !char.IsWhiteSpace(value[i - 1]))
                {
                    result.Add(' ');
                }

                result.Add(character);
            }

            return new string(result.ToArray());
        }

        private static IEnumerable<UIBehaviourTrigger> GetTriggerOptions()
        {
            return (UIBehaviourTrigger[])Enum.GetValues(typeof(UIBehaviourTrigger));
        }
    }
}



