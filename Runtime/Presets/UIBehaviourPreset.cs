using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Behaviour Preset", fileName = "UIBehaviourPreset")]
    public sealed class UIBehaviourPreset : UIPresetBase
    {
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<UIBehaviourBlock> behaviours = new List<UIBehaviourBlock>();

        public void ApplyTo(UISelectable selectable, UIPresetApplyMask overrideMask = null)
        {
            if (selectable == null)
            {
                return;
            }

            UIPresetApplyMask mask = ResolveMask(overrideMask);
            if (overrideMask == null)
            {
                mask = new UIPresetApplyMask
                {
                    mode = UIPresetApplyMode.OnlyBehaviours
                };
            }

            selectable.ApplySelectablePresetData(null, behaviours, mask, allowReplaceBehaviours: true);
        }
    }
}



