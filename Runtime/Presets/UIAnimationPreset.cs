using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.UI
{
    [CreateAssetMenu(menuName = "UI System/Presets/Animation Preset", fileName = "UIAnimationPreset")]
    public sealed class UIAnimationPreset : UIPresetBase
    {
        [HideLabel]
        public UIAnimationState animation = new UIAnimationState();
    }
}



